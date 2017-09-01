using PptReader.Models.Office;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PptReader.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            using (var model = new PptModel())
                model.SaveNotes();
        }
    }
}
