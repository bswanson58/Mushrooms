using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using ReusableBits.Wpf.Commands;

namespace ReusableBits.Wpf.ViewModelSupport {
    internal static class Extensions {
        public static string StripLeft( this string value, int length ) {
            return value.Substring(length, value.Length - length);
        }
    }

	public class AutomaticCommandBase : AutomaticPropertyBase {
		private const string ExecutePrefix		= "Execute_";
		private const string CanExecutePrefix	= "CanExecute_";

		private readonly IDictionary<string, List<string>>	mMethodMap;
		private readonly IDictionary<string, List<string>>	mCommandMap;

		protected AutomaticCommandBase() {
			mMethodMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => !method.Name.StartsWith( CanExecutePrefix )));
			mCommandMap = MapDependencies<DependsUponAttribute>( () => GetType().GetMethods().Cast<MemberInfo>().Where( method => method.Name.StartsWith( CanExecutePrefix )));

			CreateCommands();
		}

		private void CreateCommands() {
			CommandNames.ToList()
                .ForEach( name => Set( name, 
                    new DelegateCommand<object ?>( x => ExecuteCommand( name, x ), x => CanExecuteCommand( name, x ))));
		}

		private IEnumerable<string> CommandNames =>
            from method in GetType().GetMethods()
            where method.Name.StartsWith( ExecutePrefix )
            select method.Name.StripLeft( ExecutePrefix.Length );

        private void ExecuteCommand( string name, object ? parameter ) {
			var methodInfo = GetType().GetMethod( ExecutePrefix + name );
			if( methodInfo == null ) return;

			methodInfo.Invoke( this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null );
		}

		private bool CanExecuteCommand( string name, object ? parameter ) {
			var methodInfo = GetType().GetMethod( CanExecutePrefix + name );
			if( methodInfo == null ) return true;

			return (bool)(methodInfo.Invoke( this, methodInfo.GetParameters().Length == 1 ? new[] { parameter } : null ) ?? false);
		}

		protected void RaiseCanExecuteChangedEvent( string canExecuteName ) {
			var commandName = canExecuteName.StripLeft( CanExecutePrefix.Length );
			var command = Get<DelegateCommand<object>>( commandName );

            command?.RaiseCanExecuteChanged();
		}

		protected override void RaisePropertyChanged( string propertyName ) {
			base.RaisePropertyChanged( propertyName );

			ExecuteDependentMethods( propertyName );
			FireChangesOnDependentCommands( propertyName );
		}

		private void ExecuteDependentMethods( string name ) {
			if( mMethodMap.ContainsKey( name ) )
				mMethodMap[name].ToList().ForEach( ExecuteMethod );
		}

		private void ExecuteMethod( string name ) {
			var memberInfo = GetType().GetMethod( name );
			if( memberInfo == null )
				return;

			memberInfo.Invoke( this, null );
		}

		private void FireChangesOnDependentCommands( string name ) {
			if( mCommandMap.ContainsKey( name )) {
				mCommandMap[name].ToList().ForEach( RaiseCanExecuteChangedEvent );
			}
		}

		public override bool TryGetMember( GetMemberBinder binder, out object ? result ) {
			result = Get<object>( binder.Name );

			if( result != null ) {
				return true;
			}

			return base.TryGetMember( binder, out result );
		}

		public override bool TrySetMember( SetMemberBinder binder, object ? value ) {
			var result = base.TrySetMember( binder, value );
			if( result ) {
				return true;
			}

			Set( binder.Name, value );

			return true;
		}
	}
}
