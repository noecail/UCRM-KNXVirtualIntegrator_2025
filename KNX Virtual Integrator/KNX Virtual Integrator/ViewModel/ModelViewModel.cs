using KNXIntegrator.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNX_Virtual_Integrator.ViewModel
{
    public partial class MainViewModel
    {
        private IFunctionalModelDictionary _functionalModelDictionary;
        public ObservableCollection<FunctionalModel> Models { get; }
    }


}
