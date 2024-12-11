﻿using Bureau.UI.API.Models;
using Bureau.UI.Server.API;
using Bureau.UI.Server.API.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Server.Configurations
{
    public static class BureauAPIWebAppConfiguration
    {
        public static void MapBureauAPI(this WebApplication app)
        {
            RouteGroupBuilder apiBuilder = app.MapGroup("api");

            RouteGroupBuilder entryRouteBuilder = apiBuilder.MapGroup("notes");

            entryRouteBuilder.MapPost("text", NotesMethod.CreateTextNote)
                .WithName(BureauAPIRouteNames.CreateTextNote);

            entryRouteBuilder.MapGet("notes/{id}", NotesMethod.GetNoteById)
               .WithName(BureauAPIRouteNames.GetTextNoteById);
        }
    }
}
