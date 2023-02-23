using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Mushrooms.ColorAnimation;
using ReusableBits.Wpf.ViewModelLocator;

namespace Mushrooms
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        private IServiceProvider ?  mServiceProvider;

        private void OnStartup( object sender, StartupEventArgs e ) {
            ConfigureApp();

            if( mServiceProvider != null ) {
                var mainWindow = mServiceProvider.GetService<MainWindow>();

                mainWindow?.Show();
            }
        }

        private void ConfigureApp() {
            var serviceCollection = new ServiceCollection();

            ConfigureServices( serviceCollection );

            mServiceProvider = serviceCollection.BuildServiceProvider();
            ViewModelLocationProvider.SetDefaultViewModelFactory( type => mServiceProvider.GetService( type ));
        }
    
        private void ConfigureServices( IServiceCollection services ) {

            services.AddScoped<ISequenceFactory, ColorAnimationSequenceFactory>();
            services.AddSingleton<IAnimationProcessor, ColorAnimationProcessor>();

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
