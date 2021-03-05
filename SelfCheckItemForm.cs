using AutoMapper;
using HZH_Controls.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HZH_Controls.Controls;
using QualityCheckDemo;

namespace MachineryProcessingDemo
{
    public partial class SelfCheckItemForm : FrmWithOKCancel1
    {
        public SelfCheckItemForm(string productBornCode, long? staffId, string staffCode, string staffName)
        {
            InitializeComponent();
            _productBornCode = productBornCode;
            _staffId = staffId;
            _staffCode = staffCode;
            _staffName = staffName;
        }
        private static long? _staffId;
        private static string _staffCode;
        private static string _staffName;
        private static string _productBornCode;
        private static C_ProductProcessing _cProductProcessing;
        private static List<A_ProcedureSelfCheckingConfig> _aProcedureSelfCheckingConfigs;
        public static List<string> _txtNameList = new List<string>();
        public static List<string> _mbNameList = new List<string>(); 

        public Action ChangeBgColor;
        private void SelfCheckItemForm_Load(object sender, EventArgs e)
        {
            DataFill();
        }
        public void DataFill()
        {
            using (var context = new Model())
            {
                //在产品加工过程表中根据产品出生证  获取元数据
                _cProductProcessing = context.C_ProductProcessing.FirstOrDefault(s => s.ProductBornCode == _productBornCode);

                var procedureIdInt16 = Convert.ToInt16(_cProductProcessing.ProcedureID);

                //在工序自检项配置表中根据工序主键/是否启用/有效性  获得自检项数据
                //类型转换问题  数据库设计有误吗???
                _aProcedureSelfCheckingConfigs = context.A_ProcedureSelfCheckingConfig.Where(s =>
                    s.ProcedureID == procedureIdInt16 &&
                    s.IsEnable == true&&s.IsAvailable==true).OrderByDescending(s => s.IsRequired).ToList();

                //动态加载txt和lbl控件
                int tabIndex = 1;
                int localLblY = 73;
                int localTextBoxY = 70;
                foreach (var aProcedureSelfCheckingConfig in _aProcedureSelfCheckingConfigs)
                {
                    panel3.Controls.Add(new Label()
                    {
                        Location = new Point(110, localLblY),
                        Size = new Size(86, 26),
                        Text = aProcedureSelfCheckingConfig.ItemName + ':',
                        ForeColor = Color.Black,
                        TabIndex = tabIndex,
                        BackColor = Color.Transparent,
                        Font = new Font("微软雅黑", 10.8F, FontStyle.Bold,
                            GraphicsUnit.Point, ((byte)(134))),
                        Name = aProcedureSelfCheckingConfig.ItemCode + "lbl"
                    });
                    localLblY += 60;

                    panel3.Controls.Add(new TextBox()
                    {
                        Location = new Point(204, localTextBoxY),
                        Size = new Size(168, 32),
                        Name = aProcedureSelfCheckingConfig.ItemCode + "txt"
                    });
                    localTextBoxY += 60;
                    _txtNameList.Add(aProcedureSelfCheckingConfig.ItemCode + "txt");

                    if (aProcedureSelfCheckingConfig.IsRequired != null && (bool)aProcedureSelfCheckingConfig.IsRequired)
                    {
                        panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "lbl"}"].Text =
                            panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "lbl"}"].Text
                            .Insert(panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "lbl"}"].Text.Length - 1, "*");
                        panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "lbl"}"].ForeColor = Color.Red;
                        panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "txt"}"].Tag = "*";
                        _mbNameList.Add(aProcedureSelfCheckingConfig.ItemCode);
                    }

                    //在产品质量数据表中根据 订单号项目号计划好产品号出生证工序号检验类型(自检)及检测项编号  获得特定数据(检测实际值)
                    var cProductQualityData = context.C_ProductQualityData.FirstOrDefault(s =>
                        s.OrderID == _cProductProcessing.OrderID && s.ProjectID == _cProductProcessing.ProjectID && s.PlanID == _cProductProcessing.PlanID
                        && s.ProductID == _cProductProcessing.ProductID && s.ProductBornCode == _cProductProcessing.ProductBornCode &&
                        s.ProcedureID == _cProductProcessing.ProcedureID && s.CheckType == 1 && s.ItemCode == aProcedureSelfCheckingConfig.ItemCode);

                    if (cProductQualityData != null && cProductQualityData.CollectValue != null)
                        panel3.Controls[$"{aProcedureSelfCheckingConfig.ItemCode + "txt"}"].Text = cProductQualityData.CollectValue.ToString();
                }
            }
        }
        private void AddCntLogicPro()
        {
            //添加控制点过程信息
            using (var context = new Model())
            {
                var cBWuECntlLogicPro = new C_BWuE_CntlLogicPro();
                cBWuECntlLogicPro.ProductBornCode = _productBornCode;
                cBWuECntlLogicPro.ProcedureCode = _cProductProcessing.ProcedureCode;
                cBWuECntlLogicPro.ControlPointID = 5;
                cBWuECntlLogicPro.Sort = "2";
                cBWuECntlLogicPro.EquipmentCode = "2";
                cBWuECntlLogicPro.State = "1";
                cBWuECntlLogicPro.StartTime = DateTime.Now;

                context.C_BWuE_CntlLogicPro.Add(cBWuECntlLogicPro);
                context.SaveChanges();
            }
        }
        protected override void DoEnter()
        {
            if (_cProductProcessing == null)
            {
                FrmDialog.ShowDialog(this, "未检测到上线产品", "警告");
                // MessageBox.Show("未检测到上线产品");
                return;
            }

            //添加控制点过程信息
            AddCntLogicPro();

            //数据校验
            if (DataCheck())
            {
                //录入自检项数据
                InputSelfItem();

                //控制点转档
                // CntLogicTurn();
            }
        }
        private void InputSelfItem()
        {
            //判断产品质量数据表中有没有原始数据
            //如果有的话就是更新操作
            using (var context = new Model())
            {
                //在产品质量数据表中根据产品出生证和工序号/检验结果为空 来获得录入过的数据
                var cProductQualityDatas = context.C_ProductQualityData.Where(s =>
                    s.ProductBornCode == _productBornCode&&s.ProcedureCode==_cProductProcessing.ProcedureCode
                    &&s.CheckResult==null).OrderBy(s => s.ItemID).ToList();
                if (cProductQualityDatas.Count > 0)
                {
                    foreach (var cProductQualityData in cProductQualityDatas)
                    {
                        var trim = panel3.Controls[$"{cProductQualityData.ItemCode}txt"].Text.Trim();
                        decimal.TryParse(trim, out var txtDecimalValue);
                        if (string.IsNullOrEmpty(trim))
                        {
                            cProductQualityData.CollectValue = null;
                        }
                        else 
                        {
                            cProductQualityData.CollectValue = txtDecimalValue;
                        }

                        cProductQualityData.CreateTime = DateTime.Now;
                    }
                    context.SaveChanges();
                    FrmDialog.ShowDialog(this, "自检项录入成功", "录入成功");
                    ChangeBgColor();
                    // MessageBox.Show("自检项录入成功");
                    Close();
                }
                else
                {
                    //构建产品加工过程表和产品质量数据表的映射关系
                    var mapperConfiguration = new MapperConfiguration(cfg =>
                        cfg.CreateMap<C_ProductProcessing, C_ProductQualityData>());

                    //构建工序自检项配置表和产品质量数据表的映射关系
                    var mapperConfiguration1 = new MapperConfiguration(cfg =>
                        cfg.CreateMap<A_ProcedureSelfCheckingConfig, C_ProductQualityData>());

                    foreach (var aProcedureSelfCheckingConfig in _aProcedureSelfCheckingConfigs)
                    {
                        //automapper
                        var mapper1 = mapperConfiguration1.CreateMapper();
                        var cProductQualityData = mapper1.Map<C_ProductQualityData>(aProcedureSelfCheckingConfig);
                        //automapper
                        var mapper = mapperConfiguration.CreateMapper();
                        var productQualityData = mapper.Map<C_ProductProcessing, C_ProductQualityData>(_cProductProcessing, cProductQualityData);

                        //录入值
                        var txtValue = panel3.Controls.Find(aProcedureSelfCheckingConfig.ItemCode + "txt", false).First().Text.Trim();
                        var tryParse = decimal.TryParse(txtValue, out var txtDecimalValue);
                        if (string.IsNullOrEmpty(txtValue))
                        {
                            productQualityData.CollectValue = null;
                        }
                        else if (tryParse)
                        {
                            productQualityData.CollectValue = txtDecimalValue;
                        }
                        else
                        {
                            FrmDialog.ShowDialog(this, "输入数据格式不正确", "格式异常");
                            return;
                            // MessageBox.Show("输入数据格式不正确");
                        }

                        //(补充信息)
                        productQualityData.CheckType = 1;
                        productQualityData.ItemID = aProcedureSelfCheckingConfig.ID;
                        productQualityData.CheckStaffCode = _staffCode;
                        productQualityData.CheckStaffName = _staffName;
                        productQualityData.CreateTime = DateTime.Now;

                        context.C_ProductQualityData.Add(productQualityData);
                    }

                    context.SaveChanges();
                    FrmDialog.ShowDialog(this, "自检项录入成功", "录入成功");
                    ChangeBgColor();
                    Close();
                }
            }
        }
        private bool DataCheck()
        {
            bool isOk = false;

            foreach (var aProcedureSelfCheckingConfig in _aProcedureSelfCheckingConfigs)
            {
                //录入值
                var txtValue = panel3.Controls.Find(aProcedureSelfCheckingConfig.ItemCode + "txt", false).First().Text.Trim();
                var tryParse = decimal.TryParse(txtValue, out var txtDecimalValue);
                if (tryParse||string.IsNullOrEmpty(txtValue))
                {
                    isOk = true;
                }
                else
                {
                    FrmDialog.ShowDialog(this, "输入数据格式不正确", "格式异常");
                    var control = panel3.Controls.Find(aProcedureSelfCheckingConfig.ItemCode + "txt", false).First();
                    var textBox = (TextBox)control;
                    //文本框撤销  树儿nb
                    textBox.Undo();
                    return false;
                }
            }
            return isOk;
        }
    }
}
