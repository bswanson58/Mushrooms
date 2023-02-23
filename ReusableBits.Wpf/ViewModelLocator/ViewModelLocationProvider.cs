using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

// from: https://github.com/PrismLibrary/Prism/blob/master/src/Prism.Core/Mvvm/ViewModelLocationProvider.cs
// with a few cleanups.

// usage: https://prismlibrary.com/docs/viewmodel-locator.html

namespace ReusableBits.Wpf.ViewModelLocator {
    /// <summary>
    /// The ViewModelLocationProvider class locates the view model for the view that has the AutoWireViewModelChanged attached property set to true.
    /// The view model will be located and injected into the view's DataContext. To locate the view model, two strategies are used: First the ViewModelLocationProvider
    /// will look to see if there is a view model factory registered for that view, if not it will try to infer the view model using a convention based approach.
    /// This class also provides methods for registering the view model factories,
    /// and also to override the default view model factory and the default view type to view model type resolver.
    /// </summary>

    // Documentation on using the MVVM pattern is at http://go.microsoft.com/fwlink/?LinkID=288814&clcid=0x409

    public static class ViewModelLocationProvider {
        /// <summary>
        /// A dictionary that contains all the registered factories for the views.
        /// </summary>
        static readonly Dictionary<string, Func<object>> Factories = new ();

        /// <summary>
        /// A dictionary that contains all the registered ViewModel types for the views.
        /// </summary>
        static readonly Dictionary<string, Type> TypeFactories = new ();

        /// <summary>
        /// The default view model factory which provides the ViewModel type as a parameter.
        /// </summary>
        static Func<Type, object ?> mDefaultViewModelFactory = Activator.CreateInstance;

        /// <summary>
        /// ViewModelFactory that provides the View instance and ViewModel type as parameters.
        /// </summary>
        static Func<object, Type, object> ? mDefaultViewModelFactoryWithViewParameter;

        /// <summary>
        /// Default view type to view model type resolver, assumes the view model is in same assembly as the view type, but in the "ViewModels" namespace.
        /// </summary>
        static Func<Type, Type ?> mDefaultViewTypeToViewModelTypeResolver =
            viewType => {
                var viewName = viewType.FullName;
                if( viewName != null ) {
                    viewName = viewName.Replace( ".Views.", ".ViewModels." );

                    var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                    var suffix = viewName.EndsWith( "View" ) ? "Model" : "ViewModel";
                    var viewModelName = String.Format( CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName );

                    return Type.GetType( viewModelName );
                }

                return null;
            };

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory which provides the ViewModel type as a parameter.</param>
        public static void SetDefaultViewModelFactory( Func<Type, object ?> viewModelFactory ) {
            mDefaultViewModelFactory = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view model factory.
        /// </summary>
        /// <param name="viewModelFactory">The view model factory that provides the View instance and ViewModel type as parameters.</param>
        public static void SetDefaultViewModelFactory( Func<object, Type, object> ? viewModelFactory ) {
            mDefaultViewModelFactoryWithViewParameter = viewModelFactory;
        }

        /// <summary>
        /// Sets the default view type to view model type resolver.
        /// </summary>
        /// <param name="viewTypeToViewModelTypeResolver">The view type to view model type resolver.</param>
        public static void SetDefaultViewTypeToViewModelTypeResolver( Func<Type, Type ?> viewTypeToViewModelTypeResolver ) {
            mDefaultViewTypeToViewModelTypeResolver = viewTypeToViewModelTypeResolver;
        }

        /// <summary>
        /// Automatically looks up the viewmodel that corresponds to the current view, using two strategies:
        /// It first looks to see if there is a mapping registered for that view, if not it will fallback to the convention based approach.
        /// </summary>
        /// <param name="view">The dependency object, typically a view.</param>
        /// <param name="setDataContextCallback">The call back to use to create the binding between the View and ViewModel</param>
        public static void AutoWireViewModelChanged( object view, Action<object, object> setDataContextCallback ) {
            // Try mappings first
            object ? viewModel = GetViewModelForView(view);

            // try to use ViewModel type
            if( viewModel == null ) {
                //check type mappings
                var viewModelType = GetViewModelTypeForView(view.GetType());

                // fallback to convention based
                if( viewModelType == null )
                    viewModelType = mDefaultViewTypeToViewModelTypeResolver( view.GetType() );

                if( viewModelType == null )
                    return;

                viewModel = mDefaultViewModelFactoryWithViewParameter != null ? mDefaultViewModelFactoryWithViewParameter( view, viewModelType ) : mDefaultViewModelFactory( viewModelType );
            }

            if( viewModel != null ) {
                setDataContextCallback( view, viewModel );
            }
        }

        /// <summary>
        /// Gets the view model for the specified view.
        /// </summary>
        /// <param name="view">The view that the view model wants.</param>
        /// <returns>The ViewModel that corresponds to the view passed as a parameter.</returns>
        private static object ? GetViewModelForView( object view ) {
            var viewKey = view.GetType().ToString();

            // Mapping of view models base on view type (or instance) goes here
            if( Factories.ContainsKey( viewKey ) )
                return Factories[viewKey]();

            return null;
        }

        /// <summary>
        /// Gets the ViewModel type for the specified view.
        /// </summary>
        /// <param name="view">The View that the ViewModel wants.</param>
        /// <returns>The ViewModel type that corresponds to the View.</returns>
        private static Type ? GetViewModelTypeForView( Type view ) {
            var viewKey = view.ToString();

            if( TypeFactories.ContainsKey( viewKey ) )
                return TypeFactories[viewKey];

            return null;
        }

        /// <summary>
        /// Registers the ViewModel factory for the specified view type.
        /// </summary>
        /// <typeparam name="T">The View</typeparam>
        /// <param name="factory">The ViewModel factory.</param>
        public static void Register<T>( Func<object> factory ) {
            Register( typeof( T ).ToString(), factory );
        }

        /// <summary>
        /// Registers the ViewModel factory for the specified view type name.
        /// </summary>
        /// <param name="viewTypeName">The name of the view type.</param>
        /// <param name="factory">The ViewModel factory.</param>
        public static void Register( string viewTypeName, Func<object> factory ) {
            Factories[viewTypeName] = factory;
        }

        /// <summary>
        /// Registers a ViewModel type for the specified view type.
        /// </summary>
        /// <typeparam name="T">The View</typeparam>
        /// <typeparam name="TVm">The ViewModel</typeparam>
        public static void Register<T, TVm>() {
            var viewType = typeof(T);
            var viewModelType = typeof( TVm );

            Register( viewType.ToString(), viewModelType );
        }

        /// <summary>
        /// Registers a ViewModel type for the specified view.
        /// </summary>
        /// <param name="viewTypeName">The View type name</param>
        /// <param name="viewModelType">The ViewModel type</param>
        public static void Register( string viewTypeName, Type viewModelType ) {
            TypeFactories[viewTypeName] = viewModelType;
        }
    }
}