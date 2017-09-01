using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextSpeaker
{
    class MainViewModel : INotifyPropertyChanged
    {
        private SpeakModel speak = new SpeakModel();

        public string Text { get => this.speak.Text; set => this.speak.Text = value; }

        public MainViewModel()
        {
            this.speak.PropertyChanged += this.RaisePropertyChanged;
        }

        public RelayCommand ConvertCommand => this._convertCommand = this._convertCommand ?? new RelayCommand(() => this.speak.Convert());
        private RelayCommand _convertCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) => this.OnPropertyChanged(e.PropertyName);
    }

    class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action act;
        public RelayCommand(Action act) => this.act = act;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => this.act();
    }
}
