using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AulasSiencb2.Models;

namespace AulasSiencb2.ViewModel
{
    internal class ApplicationState : INotifyPropertyChanged
    {
        private Session? _session;
        public JsonObject _data;
        private List<Session> _sessionsIn;
        private List<Session> _sessionsOut;

        public ApplicationState()
        {
            _sessionsIn = new List<Session>();
            _sessionsOut = new List<Session>();
        }

        public List<Session> SessionsIn
        {
            get { return _sessionsIn; }
            set { _sessionsIn = value; }
        }

        public List<Session> SessionOut
        {
            get { return _sessionsOut; }
            set { _sessionsOut = value; }
        }

        public Session? Session
        {
            get { return _session; }
            set
            {
                _session = value;
                OnPropertyChanged(nameof(Session));
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
