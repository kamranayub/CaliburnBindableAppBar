using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caliburn.Micro.BindableAppBar
{
    public class PanoramaSceen : Screen
    {
        public override bool Equals(object obj)
        {
            if ((obj != null) && (obj.GetType() == typeof(PanoramaItem)))
            {
                var thePanoItem = (PanoramaItem)obj;

                return base.Equals(thePanoItem.Header);
            }
            else
            {
                return base.Equals(obj);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
