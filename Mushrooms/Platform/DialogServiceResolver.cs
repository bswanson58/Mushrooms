using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Platform {
    public class DialogServiceResolver : IDialogServiceContainer {
        private readonly IServiceProvider   mServiceProvider;

        public DialogServiceResolver( IServiceProvider serviceProvider ) {
            mServiceProvider = serviceProvider;
        }

        public T Resolve<T>() where T : notnull =>
            mServiceProvider.GetRequiredService<T>();

        public T Resolve<T>( string name ) where T : notnull {
            var type = ByName( name );

            if( type == null ) {
                throw new ApplicationException( $"Type named: '{name}' could not be found" );
            }

            return (T)mServiceProvider.GetRequiredService( type );
        }

        private static Type ? ByName( string name ) =>
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault( t => t.Name.Contains( name ));
    }
}
