using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityCheckDemo.helper
{
    public class CustomCellIcon
    {
        private C_CheckTask checkTask;

        public C_CheckTask DataSource
        {
            get; 
        }

        public void SetBindSource(C_CheckTask cCheckTask)
        {
            using (var context = new Model())
            {
                checkTask = cCheckTask;
                var aProductBase = context.A_ProductBase.FirstOrDefault(s=>s.ProductCode==checkTask.ProductCode&&s.IsAvailable==true);
                if (aProductBase != null)
                {
                    var memoryStream = new MemoryStream(aProductBase.Image);
                    var fromStream = Image.FromStream(memoryStream);
                }

            }
        }

    }
}
