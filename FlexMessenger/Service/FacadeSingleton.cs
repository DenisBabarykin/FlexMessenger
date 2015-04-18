using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public sealed class FacadeSingleton
    {
        private static volatile Facade instance;
        private static object syncRoot = new Object();

        private FacadeSingleton() { }

        public static Facade Instance
        {
            get
            {
                if (instance == null)
                    lock (syncRoot)
                        if (instance == null)
                            instance = new Facade();
                return instance;
            }
        }
    }
}
