using System;
using System.Windows.Input;

namespace NI_Interface
{
    public class RelayCommand : ICommand
    {
        private Action<Object> _action;
		private Func<Object, bool> _func;

        public RelayCommand(Action<Object> action) : this(action, (obj) => { return true; })
        {
        }

		public RelayCommand(Action<Object> action, Func<Object, bool> func)
		{
			_action = action;
			_func = func;
		}

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _func(parameter);
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        #endregion

        public event EventHandler CanExecuteChanged;

		public void RaiseCanExecuteChanged()
		{
            //CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
