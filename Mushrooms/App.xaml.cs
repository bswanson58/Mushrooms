using System;
using System.Threading.Tasks;
using System.Windows;
using Fluxor;
using HueLighting.Hub;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mushrooms.Database;
using Mushrooms.Platform;
using Mushrooms.SceneBuilder.Store;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelLocator;

namespace Mushrooms {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        // designated startup method in App.xaml
        private async void OnStartup( object sender, StartupEventArgs e ) {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices( ( _, services ) => {
                    ConfigureServices( services );
                })
                .Build();

            await InitializeApp( builder.Services );
            StartApp( builder.Services );

            await builder.RunAsync();
        }

        private void StartApp( IServiceProvider serviceProvider ) {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            var lifetime = serviceProvider.GetService<IHostApplicationLifetime>();

            if( mainWindow != null ) {
                mainWindow.Closing += ( _, _ ) => lifetime?.StopApplication();

                mainWindow.Show();
            }
        }

        private async Task InitializeApp( IServiceProvider serviceProvider ) {
            ViewModelLocationProvider.SetDefaultViewModelFactory( serviceProvider.GetService );

            var store = serviceProvider.GetService<IStore>();

            if( store != null ) {
                await store.InitializeAsync();
            }
        }
    
        private static void ConfigureServices( IServiceCollection services ) {
            services.AddLogging();
            services.AddScoped<IBasicLog, BasicLog>();

            services.AddScoped<IPaletteProvider, PaletteProvider>();
            services.AddScoped<ISceneProvider, SceneProvider>();

            services.AddSingleton<MushroomGarden>();
            services.AddSingleton<IMushroomGarden>( p => p.GetRequiredService<MushroomGarden>());
            services.AddHostedService( p => p.GetRequiredService<MushroomGarden>());

            services.AddScoped<IApplicationConstants, ApplicationConstants>();
            services.AddScoped<IEnvironment, OperatingEnvironment>();
            services.AddScoped<IFileWriter, JsonObjectWriter>();
            services.AddScoped<IPreferences, PreferencesManager>();

            services.AddScoped<IDialogService, DialogService>();
            services.AddScoped<IDialogWindow, DialogWindow>();
            services.AddScoped<IDialogServiceContainer, DialogServiceResolver>();

            services.AddScoped<ISceneFacade, SceneFacade>();

            services.AddSingleton<IHubManager, HubManager>();

            services.AddFluxor( options => options.ScanAssemblies( typeof( App ).Assembly ));

            services.Scan( scan => 
                scan.FromAssembliesOf( typeof( App ), typeof( HubManager ))
                    .AddClasses( c => c.Where( classType => classType.FullName?.EndsWith( "View" ) == true ||
                                                            classType.FullName?.EndsWith( "ViewModel" ) == true ||
                                                            classType.FullName?.EndsWith( "Window" ) == true ||
                                                            classType.FullName?.EndsWith( "Dialog" ) == true ))
                    .AsSelf()
                    .WithScopedLifetime());
        }
    }
}
