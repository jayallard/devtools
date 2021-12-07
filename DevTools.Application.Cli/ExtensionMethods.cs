using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace DevTools.Application.Cli;

public static class ExtensionMethods
{
    public static Command SetHandler(this Command command, ICommandHandler handler)
    {
        command.Handler = handler;
        return command;
    }
    
    public static Option<T> SetRequired<T>(this Option<T> option)
    {
        option.IsRequired = true;
        return option;
    }
    
    public static Option<T> SetDescription<T>(this Option<T> option, string description)
    {
        option.Description = description;
        return option;
    }
}