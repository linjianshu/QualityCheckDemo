using HZH_Controls;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QualityCheckDemo;

namespace MachineryProcessingDemo
{
    public class UCTestGridTable_CustomCellIcon : UserControl, HZH_Controls.Controls.IDataGridViewCustomCell
    {
        public UCTestGridTable_CustomCellIcon()
        {
            InitializeComponent();
        }
        private C_CheckTask m_object = null;
        public object DataSource
        {
            get
            {
                return m_object;
            }
        }
        public void SetBindSource(object obj)
        {
            if (obj is C_CheckTask checkTask)
            {
                m_object = checkTask;
                using (var context = new Model())
                {
                    var aProductBase = context.A_ProductBase.FirstOrDefault(s => s.ProductCode == checkTask.ProductCode && s.IsAvailable == true);
                    if (aProductBase != null)
                    {
                        var memoryStream = new MemoryStream(aProductBase.Image);
                        var fromStream = Image.FromStream(memoryStream);
                        this.BackgroundImage = fromStream;
                        this.BackgroundImageLayout = ImageLayout.Zoom;
                    }
                }
            }
        }


        public event HZH_Controls.Controls.DataGridViewRowCustomEventHandler RowCustomEvent;

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // UCTestGridTable_CustomCellIcon
            // 
            this.Name = "UCTestGridTable_CustomCellIcon";
            this.Size = new System.Drawing.Size(37, 38);
            this.ResumeLayout(false);

        }
    }
}
