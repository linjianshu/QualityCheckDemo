using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using AutoMapper;
using HZH_Controls;
using HZH_Controls.Controls;
using HZH_Controls.Forms;
using MachineryProcessingDemo;
using Microsoft.Extensions.Configuration;

namespace QualityCheckDemo.Forms
{
    public partial class MainPanel : FrmBase
    {
        public MainPanel(long? staffId, string staffCode, string staffName)
        {
            InitializeComponent();
            _staffId = staffId;
            _staffCode = staffCode;
            _staffName = staffName;
            EmployeeIDTxt.Text = staffCode;
            EmployeeNameTxt.Text = staffName;
        }

        private static string _workshopId;
        private static string _workshopCode;
        private static string _workshopName;
        private static string _equipmentId;
        private static string _equipmentCode;
        private static string _equipmentName;
        private static long? _staffId;
        private static string _staffCode;
        private static string _staffName;

        //图标间宽度
        private static int _widthX = 1300;
        //tuple计数器
        private static int _tupleI = 0;
        //全局静态只读tuple
        private static readonly List<Tuple<string, string>> MuneList = new List<Tuple<string, string>>()
        {
            // new Tuple<string, string>("工量具", "E_icon_tools"),
            // new Tuple<string, string>("工量具", "A_fa_wrench"),
            // new Tuple<string, string>("扫描枪状态", "A_fa_check_circle"),
            // new Tuple<string, string>("扫描枪状态", "E_icon_check_alt2"),
            // new Tuple<string, string>("上线", "E_arrow_carrot_2up_alt"),
            // new Tuple<string, string>("程序文件", "A_fa_file_text"),
            // new Tuple<string, string>("程序文件", "A_fa_list_alt"),
            // new Tuple<string, string>("作业指导", "A_fa_github"),
            // new Tuple<string, string>("自检项录入", "A_fa_edit"),
            // new Tuple<string, string>("自检项录入", "E_icon_pencil_edit"),
            // new Tuple<string, string>("下线", "E_arrow_carrot_2down_alt2"),
            // new Tuple<string, string>("强制下线", "A_fa_stack_overflow"),
            // new Tuple<string, string>("刷新", "A_fa_refresh"),
            new Tuple<string, string>("切换账号", "E_arrow_left_right_alt"),
            new Tuple<string, string>("退出", "A_fa_power_off"),
            // new Tuple<string, string>("人员信息", "A_fa_address_card_o"),
        };
        private static C_CheckProcessing _cCheckProcessing;
        private void MainPanel_Load(object sender, EventArgs e)
        {
            var addXmlFile = new ConfigurationBuilder().SetBasePath("E:\\project\\visual Studio Project\\QualityCheckDemo")
                .AddXmlFile("config.xml");
            var configuration = addXmlFile.Build();
            _workshopId = configuration["WorkshopID"];
            _workshopCode = configuration["WorkshopCode"];
            _workshopName = configuration["WorkshopName"];
            _equipmentId = configuration["EquipmentID"];
            _equipmentCode = configuration["EquipmentCode"];
            _equipmentName = configuration["EquipmentName"];

            //使用hzh控件自带的图标库 tuple

            //解析tuple 加载顶部菜单栏 绑定事件
            var switchAccountLabel = GenerateLabel();
            switchAccountLabel.Click += OpenLoginForm;

            var exitLabel = GenerateLabel();
            exitLabel.Click += CloseForms;

            // 加载人员信息图标
            var tuple1 = new Tuple<string, string>("人员信息", "A_fa_address_card_o");
            var icon1 = (FontIcons)Enum.Parse(typeof(FontIcons), tuple1.Item2);
            var pictureBox1 = new PictureBox
            {
                AutoSize = false,
                Size = new Size(240, 160),
                ForeColor = Color.FromArgb(255, 77, 59),
                Image = FontImages.GetImage(icon1, 64, Color.FromArgb(255, 77, 59)),
                Location = new Point(110, 20)
            };
            PersonnelInfoPanel.Controls.Add(pictureBox1);

            // 加载箭头图标
            var tuple2 = new Tuple<string, string>("Arrow", "A_fa_arrow_down");
            var icon2 = (FontIcons)Enum.Parse(typeof(FontIcons), tuple2.Item2);
            int localY = 72;
            for (var i = 0; i < 2; i++)
            {
                ProductionStatusInfoPanel.Controls.Add(new PictureBox()
                {
                    AutoSize = false,
                    Size = new Size(40, 40),
                    ForeColor = Color.FromArgb(255, 77, 59),
                    Image = FontImages.GetImage(icon2, 40, Color.FromArgb(255, 77, 59)),
                    Location = new Point(270, localY)
                });
                localY += 98;
            }

            //修改自定义控件label.text文本
            CompletedTask1.label1.Text = " 已完成任务";
            ProductionTaskQueue1.label1.Text = "质检任务队列";

            InitialDidTasks();

            ucSignalLamp1.LampColor = new Color[] { Color.Green };
            ucSignalLamp2.LampColor = new Color[] { Color.Red };

            InialToDoTasks();

            //初始化生产状态信息面板
            using (var context = new Model())
            {
                //这里需要配置修改xml
                var cBBdbRCntlPntBases = context.C_BBdbR_CntlPntBase.Where(s =>
                        s.CntlPntTyp == "2" && s.Enabled == "1")
                    .OrderBy(s => s.CntlPntSort).ToList();

                int localLblY = 25;
                foreach (var cBBdbRCntlPntBase in cBBdbRCntlPntBases)
                {
                    var label = new Label()
                    {
                        Location = new Point(239, localLblY),
                        Size = new Size(112, 39),
                        Name = cBBdbRCntlPntBase.CntlPntCd,
                        BackColor = Color.LightSlateGray,
                        Font = new Font("微软雅黑", 10.8F, FontStyle.Bold,
                            GraphicsUnit.Point, ((byte)(134))),
                        Text = cBBdbRCntlPntBase.CntlPntNm,
                        TextAlign = ContentAlignment.MiddleCenter,
                    };
                    if (label.Name.Equals("control001"))
                    {
                        label.Click += BeginQcEvent;
                    }
                    else if (label.Name.Equals("control002"))
                    {
                        label.Click += UpLoadReportEvent;
                    }
                    else if (label.Name.Equals("control003"))
                    {
                        label.Click += EndQcEvent;
                    }
                    else
                    {
                        //需要修改
                        label.Click += QualityCheckEvent;
                    }
                    ProductionStatusInfoPanel.Controls.Add(label);
                    localLblY += 96;
                }
            }

            //获取当前质检中心的质检任务(已上线)
            using (var context = new Model())
            {
                var cCheckProcessing = context.C_CheckProcessing.FirstOrDefault(s => s.EquipmentID == _equipmentId && s.OnlineTime != null);
                if (cCheckProcessing != null)
                {
                    ProductIDTxt.Text = cCheckProcessing.ProductBornCode;
                    ProductIDTxt.ReadOnly = true;
                    ProductNameTxt.Text = cCheckProcessing.ProductName;
                    ProductNameTxt.ReadOnly = true;
                    CurrentProcessTxt.Text = cCheckProcessing.ProcedureName + " ---- 三坐标检验 ";
                    CurrentProcessTxt.ReadOnly = true;
                    QCTimeTxt.Text = cCheckProcessing.OnlineTime.ToString();
                    QCTimeTxt.ReadOnly = true;
                    ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                        Color.MediumSeaGreen;
                }
                if (!string.IsNullOrEmpty(ProductIDTxt.Text))
                {
                    //在质检过程表中根据产品出生证  获取元数据
                    _cCheckProcessing = context.C_CheckProcessing.FirstOrDefault(s => s.ProductBornCode == ProductIDTxt.Text.Trim());
                }
            }

            //初始化判断质检文件上传完成与否
            ReportUploadJudge();

            timer1.Enabled = true;
        }

        private void OpenForceOfflineForm(object sender, EventArgs e)
        {
            InialToDoTasks();
        }

        private void EndQcEvent(object sender, EventArgs e)
        {
            var label = (Label)sender;
            if (label.BackColor==Color.MediumSeaGreen)return; 
                panel10.Controls.Clear();
            if (string.IsNullOrEmpty(ProductIDTxt.Text))
            {
                FrmDialog.ShowDialog(this, "未检测到上线质检产品", "警告");
                return;
            }
            var backColor = ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor;
            if (backColor != Color.MediumSeaGreen)
            {
                FrmDialog.ShowDialog(this, "请先上传质检报告", "提示");
                return;
            }
            // ProductionStatusInfoPanel.Controls.Find("control003")
            UploadCntLogicTurn();
            OpenScanOfflineForm(out var isOk);
            if (isOk)
            {
                AddCntLogicProOffline();
            }
        }
        private void InialToDoTasks()
        {
            //  自定义表格 装载图片等资源
            List<DataGridViewColumnEntity> lstColumns1 = new List<DataGridViewColumnEntity>
            {
                new DataGridViewColumnEntity()
                {
                    DataField = "ProductBornCode", HeadText = "产品出生证", Width = 25, WidthType = SizeType.Percent
                },
                new DataGridViewColumnEntity()
                {
                    DataField = "ProcedureName", HeadText = "工序名称", Width = 15, WidthType = SizeType.Percent
                },
                new DataGridViewColumnEntity()
                {
                    DataField = "CreateTime", HeadText = "预计开始时间", Width = 35, WidthType = SizeType.Percent
                },
                new DataGridViewColumnEntity()
                {
                    DataField = "Reserve1", HeadText = "检验类型", Width = 25, WidthType = SizeType.Percent
                }
            };
            ucDataGridView2.Columns = lstColumns1;
            ucDataGridView2.ItemClick += UcDataGridView2_ItemClick;

            //拿到待加工产品排序集合
            var toDoProcedureTask = GetToDoProcedureTask();
            ucDataGridView2.DataSource = toDoProcedureTask;
        }

        private void InitialDidTasks()
        {
            // 自定义表格 装载图片等资源
            List<DataGridViewColumnEntity> lstColumns = new List<DataGridViewColumnEntity>
            {
                new DataGridViewColumnEntity()
                {
                    DataField = "ProductBornCode", HeadText = "产品出生证", Width = 40, WidthType = SizeType.Percent
                },
                new DataGridViewColumnEntity()
                {
                    DataField = "Reserve2", HeadText = "工序名称", Width = 20, WidthType = SizeType.Percent
                },
                new DataGridViewColumnEntity()
                {
                    DataField = "Reserve1", HeadText = "质检结果", Width = 35, WidthType = SizeType.Percent
                }
            };

            var didProcedureTask = GetDidProcedureTask();
            ucDataGridView1.Columns = lstColumns;
            ucDataGridView1.DataSource = didProcedureTask;
        }
        private void UcDataGridView2_ItemClick(object sender, DataGridViewEventArgs e)
        {
            ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                Color.LightSlateGray;
            ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor =
                Color.LightSlateGray;
            ProductionStatusInfoPanel.Controls.Find("control003", false).First().BackColor =
                Color.LightSlateGray;

            var controls = panel10.Controls.Find("scanOnlineForm", false);
            if (controls.Any())
            {
                controls[0].Dispose();
            }

            if (!HasExitProductTask())
            {
                ProductNameTxt.Clear();
                ProductIDTxt.Clear();
                CurrentProcessTxt.Clear();
                QCTimeTxt.Clear();

                var dataGridViewRow = ucDataGridView2.SelectRow;
                var dataSource = dataGridViewRow.DataSource;
                if (dataSource is C_CheckTask checktask)
                {
                    var dialogResult = FrmDialog.ShowDialog(this, $"确定上线选中产品[{checktask.ProductBornCode}]吗", "质检上线", true);
                    if (dialogResult == DialogResult.OK)
                    {
                        var scanOnlineForm = new ScanOnlineForm(_staffId, _staffCode, _staffName, checktask.ProductBornCode, _workshopId, _workshopCode, _workshopName, _equipmentId, _equipmentCode, _equipmentName)
                        {
                            DisplayInfoToMainPanel = (s1, s2, s3, s4) =>
                            {
                                ProductIDTxt.Text = s1;
                                ProductNameTxt.Text = s2;
                                CurrentProcessTxt.Text = s3;
                                QCTimeTxt.Text = s4;
                            },
                            ChangeBgColor = () =>
                            {
                                ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                                    Color.MediumSeaGreen;
                                ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor =
                                    Color.LightSlateGray;
                                ProductionStatusInfoPanel.Controls.Find("control003", false).First().BackColor =
                                    Color.LightSlateGray;
                            }
                        };
                        if (scanOnlineForm.CheckTaskValidity(checktask.ProcedureCode))
                        {
                            scanOnlineForm.AddCntLogicPro(checktask.ProcedureCode);
                            {
                                //操作人员确认
                                if (scanOnlineForm.WorkerConfirm())
                                {
                                    //转档  检验任务表=>检验过程表
                                    scanOnlineForm.CheckProcessTurnArchives();
                                    //完善检验任务表 诸如任务状态 ; 修改人修改时间
                                    scanOnlineForm.PerfectCheckTask();
                                    //控制点转档  
                                    scanOnlineForm.CntLogicTurn();
                                    InialToDoTasks();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                    Color.MediumSeaGreen;
                ReportUploadJudge();
            }
        }
        private List<C_CheckTask> GetDidProcedureTask()
        {
            var cCheckTasks = new List<C_CheckTask>();
            using (var context = new Model())
            {
                //在检验任务表中根据设备编号/有效性/任务状态/质检类型(三坐标) 修改时间排序
                var checkTasks = context.C_CheckTask
                    .Where(s => s.IsAvailable == true && s.TaskState == (decimal?)CheckTaskState.Completed &&
                                s.CheckType == (decimal?)CheckType.ThreeCoordinate).OrderBy(s => s.LastModifiedTime).ToList();

                foreach (var cCheckTask in checkTasks)
                {
                    //令apsdetail的reserve2 字段作为工序名称来搞
                    //在产品工序基础表中根据产品号/有效性  获得工序名称
                    cCheckTask.Reserve2 = context.A_ProductProcedureBase
                        .Where(s => s.IsAvailable == true && s.ProductID == cCheckTask.ProductID &&
                                    s.PlanID == cCheckTask.PlanID && s.ProcedureCode == cCheckTask.ProcedureCode)
                        .Select(s => s.ProcedureName).FirstOrDefault();

                    //在检验档案表里根据产品产品出生证/检验类型(三坐标)/工序编号 获得检验结果(正常/NG)
                    var cCheckProcessingDocument = context.C_CheckProcessingDocument.FirstOrDefault(s =>
                        s.ProductBornCode == cCheckTask.ProductBornCode && s.CheckType == (decimal?)CheckType.ThreeCoordinate &&
                        s.ProcedureCode == cCheckTask.ProcedureCode);
                    cCheckTask.Reserve1 = cCheckProcessingDocument.Offline_type == (decimal?)OfflineType.Normal ? "正常下机" : "NG下机";
                }

                cCheckTasks.AddRange(checkTasks);
                return cCheckTasks;
            }
        }
        private List<C_CheckTask> GetToDoProcedureTask()
        {
            var cCheckTasks = new List<C_CheckTask>();
            using (var context = new Model())
            {
                //在质检任务表中根据设备编号/任务状态(没完成)/有效性/质检类型(三坐标) 按创建时间排个序(先创建的先排序)
                var checkTasks = context.C_CheckTask
                    .Where(s => s.TaskState != (decimal?)CheckTaskState.Completed && s.IsAvailable == true
                    && s.CheckType == (decimal?)CheckType.ThreeCoordinate)
                    .OrderBy(s => s.CreateTime).ToList();

                foreach (var cCheckTask in checkTasks)
                {
                    cCheckTask.Reserve1 = "三坐标检验";
                }
                cCheckTasks.AddRange(checkTasks);
                return cCheckTasks;
            }
        }
        private void ReportUploadJudge()
        {
            using (var context = new Model())
            {
                //在质检过程表中根据产品出生证  获取元数据
                _cCheckProcessing = context.C_CheckProcessing.FirstOrDefault(s => s.ProductBornCode == ProductIDTxt.Text.Trim());

                if (_cCheckProcessing != null)
                {
                    //前提是 工序编号里没有空格这样的特殊字符
                    var strings = CurrentProcessTxt.Text.Trim().Split(' ')[0];
                    var any = context.C_CheckProcessing.Any(s =>
                        s.ProductBornCode == ProductIDTxt.Text.Trim() &&
                        s.ProcedureName == strings &&
                        s.CheckType == (decimal?)CheckType.ThreeCoordinate && s.CheckReportPath != null);
                    if (any)
                    {
                        ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor
                            = Color.MediumSeaGreen;
                    }
                }

            }

        }
        public bool HasExitProductTask()
        {
            using (var context = new Model())
            {
                //在质检过程表中根据设备编号/上线时间判断是否存在正在质检的任务
                var cCheckProcessing =
                    context.C_CheckProcessing.FirstOrDefault(s => s.OnlineTime != null && s.CheckType == (decimal?)CheckType.ThreeCoordinate);
                if (cCheckProcessing != null)
                {
                    FrmDialog.ShowDialog(this, "当前已有正在处理的质检任务,请完成", "已有质检任务");
                    return true;
                }
                return false;
            }
        }
        private void BeginQcEvent(object sender, EventArgs e)
        {
            var find = panel10.Controls.Find("scanOnlineForm", false);
            if (find.Any())
            {
                return;
            }
            ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                Color.LightSlateGray;
            ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor =
                Color.LightSlateGray;
            ProductionStatusInfoPanel.Controls.Find("control003", false).First().BackColor =
                Color.LightSlateGray;
            var exitProductTask = HasExitProductTask();
            if (!exitProductTask)
            {
                ProductNameTxt.Clear();
                ProductIDTxt.Clear();
                CurrentProcessTxt.Clear();
                QCTimeTxt.Clear();
                var scanOnlineForm = new ScanOnlineForm(_staffId, _staffCode, _staffName)
                {
                    DisplayInfoToMainPanel = (s1, s2, s3, s4) =>
                    {
                        ProductIDTxt.Text = s1;
                        ProductNameTxt.Text = s2;
                        CurrentProcessTxt.Text = s3;
                        QCTimeTxt.Text = s4;
                    },
                    ChangeBgColor = () =>
                    {
                        ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                                Color.MediumSeaGreen;
                        ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor =
                            Color.LightSlateGray;
                        ProductionStatusInfoPanel.Controls.Find("control003", false).First().BackColor =
                            Color.LightSlateGray;
                    },
                    RegetProcedureTasksDetails = () =>
                    {
                        InialToDoTasks();
                    }
                };
                var controls = scanOnlineForm.Controls.Find("lblTitle", false).First();
                controls.Visible = false;
                scanOnlineForm.Location = new Point(panel10.Width / 2 - scanOnlineForm.Width / 2, 0);
                scanOnlineForm.FormBorderStyle = FormBorderStyle.None;
                scanOnlineForm.AutoSize = false;
                scanOnlineForm.AutoScaleMode = AutoScaleMode.None;
                scanOnlineForm.Size = new Size(553, panel10.Height);
                scanOnlineForm.AutoScaleMode = AutoScaleMode.Font;
                scanOnlineForm.TopLevel = false;
                scanOnlineForm.BackColor = Color.FromArgb(247, 247, 247);
                scanOnlineForm.ForeColor = Color.FromArgb(66, 66, 66);
                panel10.Controls.Add(scanOnlineForm);
                scanOnlineForm.Show();
            }
            else
            {
                ProductionStatusInfoPanel.Controls.Find("control001", false).First().BackColor =
                    Color.MediumSeaGreen;
                ReportUploadJudge();
            }
        }
        private void UpLoadReportEvent(object sender, EventArgs e)
        {
            panel10.Controls.Clear();
            if (string.IsNullOrEmpty(ProductIDTxt.Text))
            {
                FrmDialog.ShowDialog(this, "未检测到上线质检产品", "警告");
                return;
            }
            using (var context = new Model())
            {
                //在质检加工过程表中根据产品出生证  获取元数据
                _cCheckProcessing = context.C_CheckProcessing.FirstOrDefault(s => s.ProductBornCode == ProductIDTxt.Text.Trim());
            }

            if (_cCheckProcessing != null)
            {
                AddUploadCntLogic();
                var selectUploadFile = SelectUploadFile(out string filePath);
                if (selectUploadFile == DialogResult.OK)
                {
                    UploadFilePath(filePath);
                    ProductionStatusInfoPanel.Controls.Find("control002", false).First().BackColor =
                        Color.MediumSeaGreen;
                    FrmDialog.ShowDialog(this, "质检报告上传成功");
                }
            }
            else
            {
                FrmDialog.ShowDialog(this, "未检测到上线质检产品", "警告");
            }
        }
        private void AddUploadCntLogic()
        {
            using (var context = new Model())
            {
                var cBWuECntlLogicPro = new C_BWuE_CntlLogicPro
                {
                    ProcedureCode = _cCheckProcessing.ProcedureCode,
                    ProductBornCode = _cCheckProcessing.ProductBornCode,
                    ControlPointID = 5,
                    Sort = "2",
                    EquipmentCode = _equipmentCode,
                    State = "1",
                    StartTime = context.GetServerDate()
                };
                context.C_BWuE_CntlLogicPro.Add(cBWuECntlLogicPro);
                context.SaveChanges();
            }
        }
        private void UploadFilePath(string filePath)
        {
            using (var context = new Model())
            {
                //在质量数据表中根据计划号/产品出生证/工序编号/检验类型(三坐标)
                var cProductQualityData = context.C_ProductQualityData.FirstOrDefault(s =>
                    s.PlanID == _cCheckProcessing.PlanID && s.ProductBornCode == _cCheckProcessing.ProductBornCode &&
                    s.ProcedureCode == _cCheckProcessing.ProcedureCode && s.CheckType == (decimal?)CheckType.ThreeCoordinate);

                //如果找到了就更新操作 , 如果没找到就插入
                if (cProductQualityData != null)
                {
                    //这里有疑问
                    cProductQualityData.CheckReportPath = filePath;
                    cProductQualityData.CheckStaffName = _staffName;
                    cProductQualityData.CheckStaffCode = _staffCode;
                    context.SaveChanges();
                }
                else
                {
                    var mapperConfiguration = new MapperConfiguration(cfg =>
                          cfg.CreateMap<C_CheckProcessing, C_ProductQualityData>());
                    var mapper = mapperConfiguration.CreateMapper();
                    var productQualityData = mapper.Map<C_ProductQualityData>(_cCheckProcessing);

                    productQualityData.CreateTime = context.GetServerDate();
                    productQualityData.OnlineStaffCode = _staffCode;
                    productQualityData.OnlineStaffID = _staffId;
                    productQualityData.OnlineStaffName = _staffName;

                    //这里有疑问 如果需要修改的话, 机加工那边也需要修改
                    productQualityData.CheckStaffCode = _staffCode;
                    productQualityData.CheckStaffName = _staffCode;

                    productQualityData.CheckReportPath = filePath;

                    context.C_ProductQualityData.Add(productQualityData);
                    context.SaveChanges();
                }

                context.Entry(_cCheckProcessing).State = EntityState.Modified;
                _cCheckProcessing.CheckReportPath = filePath;

                context.SaveChanges();
            }
        }
        private DialogResult SelectUploadFile(out string filePath)
        {
            filePath = "";
            var dialogResult = DialogResult.None;
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Title = "请选择上传报告";
            openFileDialog.Filter = "所有文件(*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                dialogResult = MessageBox.Show("已选择文件:" + filePath + ",确定上传该文件吗", "文件上传提示", MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);
            }
            return dialogResult;
        }
        private void AddCntLogicProOffline(string remark = "")
        {
            using (var context = new Model())
            {
                var cBWuECntlLogicPro = new C_BWuE_CntlLogicPro
                {
                    ProductBornCode = ProductIDTxt.Text.Trim(),
                    ProcedureCode = _cCheckProcessing.ProcedureCode,
                    ControlPointID = 6,
                    Sort = "3",
                    EquipmentCode = _equipmentCode,
                    State = "1",
                    StartTime = context.GetServerDate(),
                    Remarks = remark
                };

                context.Entry(cBWuECntlLogicPro).State = EntityState.Added;
                context.SaveChanges();
            }
        }
        private void OpenScanOfflineForm(out bool isOk)
        {
            var scanOfflineForm = new ScanOfflineForm(ProductIDTxt.Text.Trim(), _staffId, _staffCode, _staffName)
            {
                ChangeBgColor = () =>
                    ProductionStatusInfoPanel.Controls.
                        Find("control003", false).First().BackColor = Color.MediumSeaGreen,
                ClearMainPanelTxt = () =>
                {
                    ProductIDTxt.Clear();
                    CurrentProcessTxt.Clear();
                    ProductNameTxt.Clear();
                    QCTimeTxt.Clear();
                },
                RegetProcedureTasksDetails = () =>
                {
                    InitialDidTasks();
                    InialToDoTasks();
                }
            };
            var controls = scanOfflineForm.Controls.Find("lblTitle", false).First();
            controls.Visible = false;
            scanOfflineForm.Location = new Point(panel10.Width / 2 - scanOfflineForm.Width / 2, 0);
            scanOfflineForm.FormBorderStyle = FormBorderStyle.None;
            scanOfflineForm.AutoSize = false;
            scanOfflineForm.AutoScaleMode = AutoScaleMode.None;
            scanOfflineForm.Size = new Size(553, panel10.Height);
            scanOfflineForm.AutoScaleMode = AutoScaleMode.Font;
            scanOfflineForm.TopLevel = false;
            scanOfflineForm.BackColor = Color.FromArgb(247, 247, 247);
            scanOfflineForm.ForeColor = Color.FromArgb(66, 66, 66);
            panel10.Controls.Add(scanOfflineForm);
            scanOfflineForm.Show();
            isOk = true;
        }
        private void QualityCheckEvent(object sender, EventArgs e)
        {

        }
        private void UploadCntLogicTurn(string remark = "")
        {
            using (var context = new Model())
            {
                //在控制点过程表中根据产品出生证/工序编号/控制点id/设备编号 按开始时间排序 获得list
                var bWuECntlLogicPros = context.C_BWuE_CntlLogicPro
                    .Where(s => s.ProductBornCode == _cCheckProcessing.ProductBornCode &&
                                s.ProcedureCode == _cCheckProcessing.ProcedureCode && s.ControlPointID == 5 &&
                                s.EquipmentCode == _equipmentCode).OrderByDescending(s => s.StartTime).ToList();

                if (bWuECntlLogicPros.Any())
                {
                    bWuECntlLogicPros[0].State = "2";
                    bWuECntlLogicPros[0].FinishTime = context.GetServerDate();
                    bWuECntlLogicPros[0].Remarks = remark;
                }

                var mapperConfiguration = new MapperConfiguration(cfg =>
                                       cfg.CreateMap<C_BWuE_CntlLogicPro, C_BWuE_CntlLogicDoc>());
                var mapper = mapperConfiguration.CreateMapper();
                foreach (var cBWuECntlLogicPro in bWuECntlLogicPros)
                {
                    var cBWuECntlLogicDoc = mapper.Map<C_BWuE_CntlLogicDoc>(cBWuECntlLogicPro);
                    context.C_BWuE_CntlLogicDoc.Add(cBWuECntlLogicDoc);
                    context.C_BWuE_CntlLogicPro.Remove(cBWuECntlLogicPro);
                }

                context.SaveChanges();
            }
        }
        private void OpenLoginForm(object sender, EventArgs e)
        {
            new UserLoginForm().Show();

            // C_LoginInProcessing cLoginInProcessing;
            // using (var context = new Model())
            // {
            //     //在登陆过程表中根据员工id/设备id/下线时间非空 获得登陆过程信息
            //     cLoginInProcessing = context.C_LoginInProcessing.First(s =>
            //         s.StaffCode == EmployeeIDTxt.Text && s.EquipmentID.ToString() == _equipmentId && s.OfflineTime == null);
            //
            //     cLoginInProcessing.OfflineTime = context.GetServerDate();
            //     context.SaveChanges();
            // }
            // LoginUserTurnArchives(cLoginInProcessing);

            _tupleI = 0;
            _widthX = 1300;
            this.Close();
        }
        private void CloseForms(object sender, EventArgs e)
        {
            // C_LoginInProcessing cLoginInProcessing;
            // using (var context = new Model())
            // {
            //     cLoginInProcessing = context.C_LoginInProcessing.First(s =>
            //         s.StaffCode == EmployeeIDTxt.Text && s.EquipmentID.ToString() == _equipmentId && s.OfflineTime == null);
            //     cLoginInProcessing.OfflineTime = context.GetServerDate();
            //     context.SaveChanges();
            // }
            // LoginUserTurnArchives(cLoginInProcessing);
            Application.Exit();
        }
        private Label GenerateLabel()
        {
            var icon = (FontIcons)Enum.Parse(typeof(FontIcons), MuneList[_tupleI].Item2);
            var label = new Label
            {
                AutoSize = false,
                Size = new Size(90, 60),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.BottomCenter,
                ImageAlign = ContentAlignment.TopCenter,
                Margin = new Padding(5),
                Text = MuneList[_tupleI].Item1,
                Image = FontImages.GetImage(icon, 32, Color.White),
                Location = new Point(_widthX, 0),
                Font = new Font("微软雅黑", 12, FontStyle.Bold)
            };
            FirstTitlePanel.Controls.Add(label);
            _widthX += 90;
            _tupleI++;
            return label;
        }
        public void LoginUserTurnArchives(C_LoginInProcessing cLoginInProcessing)
        {
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<C_LoginInProcessing, C_LoginInDocument>());
            var mapper = mapperConfiguration.CreateMapper();
            var cLoginInDocument = mapper.Map<C_LoginInDocument>(cLoginInProcessing);
            using (var context = new Model())
            {
                //此处应该优化成事务操作 保证acid原则
                context.C_LoginInDocument.Add(cLoginInDocument);
                context.SaveChanges();
                context.Entry(cLoginInProcessing).State = EntityState.Deleted;
                context.SaveChanges();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var toDoProcedureTask = GetToDoProcedureTask();
            ucDataGridView2.DataSource = toDoProcedureTask;
        }
    }

    public class TestGridModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        public int Sex { get; set; }
        public int Age { get; set; }
        public List<TestGridModel> Childrens { get; set; }
    }
}
