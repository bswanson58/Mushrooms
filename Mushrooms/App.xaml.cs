using System;
using System.Threading.Tasks;
using System.Windows;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using Mushrooms.ColorAnimation;
using Mushrooms.PatternTester.Store;
using ReusableBits.Wpf.ViewModelLocator;

namespace Mushrooms {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        private IServiceProvider ?  mServiceProvider;

        private void OnStartup( object sender, StartupEventArgs e ) {
            mServiceProvider = ConfigureApp().Result;

            if( mServiceProvider != null ) {
                var mainWindow = mServiceProvider.GetService<MainWindow>();

                mainWindow?.Show();
            }
        }

        private static async Task<IServiceProvider> ConfigureApp() {
            var serviceCollection = new ServiceCollection();

            ConfigureServices( serviceCollection );

            var serviceProvider = serviceCollection.BuildServiceProvider();

            ViewModelLocationProvider.SetDefaultViewModelFactory( type => serviceProvider.GetService( type ));

            var store = serviceProvider.GetService<IStore>();

            if( store != null ) {
                await store.InitializeAsync();
            }

            return serviceProvider;
        }
    
        private static void ConfigureServices( IServiceCollection services ) {

            services.AddScoped<ISequenceFactory, ColorAnimationSequenceFactory>();
            services.AddSingleton<IAnimationProcessor, ColorAnimationProcessor>();

            services.AddScoped<IPatternTest, PatternTestFacade>();

            services.AddFluxor( options => options.ScanAssemblies( typeof( App ).Assembly ));

            services.Scan( scan => 
                scan.FromAssemblyOf<App>()
                    .AddClasses( c => c.Where( classType => classType.FullName?.EndsWith( "ViewModel" ) == true ||
                                                            classType.FullName?.EndsWith( "Window" ) == true ||
                                                            classType.FullName?.EndsWith( "Dialog" ) == true ))
                    .AsSelf()
                    .WithScopedLifetime());
        }
    }
}
