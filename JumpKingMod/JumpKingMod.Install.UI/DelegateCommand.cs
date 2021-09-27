using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JumpKingMod.Install.UI
{
    /// <summary>
    /// An implementation of <see cref="ICommand"/> which wraps a delegate call
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private readonly Predicate<object> canExecutePredicate;
        private readonly Action<object> executeAction;

        /// <summary>
        /// Ctor for creating a <see cref="DelegateCommand"/> with only an action specified, it can always execute
        /// </summary>
        public DelegateCommand(Action<object> executeAction) : this(executeAction, null)
        {

        }

        /// <summary>
        /// Ctor for creating a <see cref="DelegateCommand"/> with an action and a predicate specified
        /// </summary>
        /// <param name="executeAction">The action to execute with this command</param>
        /// <param name="canExecutePredicate">A predicate which determines if we can execute this action</param>
        public DelegateCommand(Action<object> executeAction, Predicate<object> canExecutePredicate)
        {
            this.executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
            this.canExecutePredicate = canExecutePredicate;
        }

        /// <summary>
        /// Implementation of <see cref="ICommand.Execute(object)"/>
        /// </summary>
        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        /// <summary>
        /// Implementation of <see cref="ICommand.CanExecute(object)"/>
        /// </summary>
        public bool CanExecute(object parameter)
        {
            if (canExecutePredicate != null)
            {
                return canExecutePredicate(parameter);
            }
            return true;
        }

        /// <summary>
        /// Invokes the <see cref="CanExecuteChanged"/> event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
