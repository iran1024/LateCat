using System;
using System.Diagnostics;
using System.Windows.Input;

namespace LateCat
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute)
            : this(execute, null)
        {

        }

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));

            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object? parameters)
        {
            return _canExecute == null || _canExecute(parameters);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public void Execute(object? parameters)
        {
            _execute(parameters);
        }
    }
}
