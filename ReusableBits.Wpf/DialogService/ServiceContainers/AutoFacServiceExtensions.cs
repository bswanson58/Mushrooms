/*
    public static class AutoFacBuilderExtensions {
        public static void RegisterDialog( this ContainerBuilder builder, Type dialogType ) {
            builder.RegisterType( dialogType ).Named( dialogType.Name, typeof( object ));
        }

        // Register dialogs for the DialogService.
        public static void RegisterDialog<T>( this ContainerBuilder builder ) where T : class {
            builder.RegisterType<T>().Named<object>( typeof( T ).Name );
        }
    }
*/
