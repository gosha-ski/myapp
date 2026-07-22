using Autofac;
using LibM520.Driver.HighLevel;
using System;
using System.Collections.Generic;
using System.Text;
using MyAvaloniaApp.Views;

namespace MyAvaloniaApp;

public static class Bootstrapper
{
    public static IContainer Container { get; private set; } = null!;

    public static void Build()
    {
        var builder = new ContainerBuilder();

#if NETCOREAPP3_1_OR_GREATER
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        System.Text.Encoding.GetEncoding("windows-1251");
#endif

        builder.RegisterAssemblyTypes(
                typeof(IDriver520).Assembly,
                typeof(Bootstrapper).Assembly)
            .PublicOnly()
            .AsSelf()
            .AsImplementedInterfaces();
        //PressureService
        // Твои классы
        builder.RegisterType<MeasurementService>().AsSelf().SingleInstance();
        builder.RegisterType<PressureService>().AsSelf().SingleInstance();
        builder.RegisterType<MainWindow>().AsSelf().SingleInstance();
        builder.RegisterType<DataReadingWindow>().AsSelf().SingleInstance();

        Container = builder.Build();
    }
}