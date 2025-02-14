﻿using Microsoft.Extensions.Hosting;
using SFA.DAS.Funding.ApprenticeshipEarnings.MessageHandlers;
using System;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication();

var startup = new Startup();
startup.Configure(host);

var app = host.Build();
app.Run();