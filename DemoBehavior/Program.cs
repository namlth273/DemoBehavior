using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoBehavior
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddAutoMapper(typeof(Program).Assembly);

            var builder = new ContainerBuilder();

            builder.Populate(services);

            builder.RegisterModule<AutofacModule>();

            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var mediator = scope.Resolve<IMediator>();

                await mediator.Send(new Command<GetCustomerService.MessageBody>
                {
                    Body = new GetCustomerService.MessageBody
                    {
                        Name = "Nam Le"
                    }
                });

                await mediator.Send(new Command<DownloadCustomerService.MessageBody>
                {
                    Body = new DownloadCustomerService.MessageBody
                    {
                        Name = "Nam Le"
                    }
                });
            }

            Console.WriteLine("Hello World!");
        }
    }

    public class ExceptionHandlingBehavior<TCommand, TResponse> : IPipelineBehavior<TCommand, TResponse>
    //where TCommand : ICommand<IMessageBody>
    {
        private readonly IMapper _mapper;

        public ExceptionHandlingBehavior(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var type = request.GetType().GetGenericArguments();

            var lockKey = new LockKey();

            _mapper.Map(request, lockKey);

            Console.WriteLine($"ExceptionHandlingBehavior {type.First().FullName} | LockKey {lockKey.Name}");
            return await next();
        }
    }

    public interface IMessageBody
    {
        string Name { get; set; }
    }

    public class BaseMessageBody : IMessageBody
    {
        public string Name { get; set; }
    }

    public interface ICommand<T> where T : IMessageBody
    {
        T Body { get; set; }
    }

    public class Command<T> : IRequest<CommandResponse>, ICommand<T> where T : BaseMessageBody
    {
        public T Body { get; set; }
    }

    public class CommandResponse
    {

    }

    public class BehaviorModel : BaseMessageBody
    {
    }

    public class LockKey
    {
        public string Name { get; set; }
    }

    public class MappingProfile<T> : Profile where T : BaseMessageBody
    {
        public MappingProfile()
        {
            CreateMap<Command<T>, BehaviorModel>()
                .ForMember(m => m.Name, o => o.MapFrom(f => f.Body.Name));
        }
    }

    public class GetCustomerService
    {
        public class MessageBody : BaseMessageBody
        {
        }

        public class Command : IRequest<CommandResponse>, ICommand<MessageBody>
        {
            public MessageBody Body { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command<MessageBody>, CommandResponse>
        {
            public async Task<CommandResponse> Handle(Command<MessageBody> request, CancellationToken cancellationToken)
            {
                Console.WriteLine("Handled GetCustomerService");

                return new CommandResponse();
            }
        }

        public class MappingProfile : MappingProfile<MessageBody>
        {
            public MappingProfile()
            {
                CreateMap<Command<MessageBody>, LockKey>()
                    .ForMember(m => m.Name, o => o.MapFrom(f => f.Body.Name));
            }
        }
    }

    public class DownloadCustomerService
    {
        public class MessageBody : BaseMessageBody
        {
        }

        public class Command : IRequest<CommandResponse>, ICommand<MessageBody>
        {
            public MessageBody Body { get; set; }
        }

        public class CommandHandler : IRequestHandler<Command<MessageBody>, CommandResponse>
        {
            public async Task<CommandResponse> Handle(Command<MessageBody> request, CancellationToken cancellationToken)
            {
                Console.WriteLine("Handled DownloadCustomerService");

                return new CommandResponse();
            }
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                CreateMap<Command<DownloadCustomerService.MessageBody>, LockKey>()
                    .ForMember(m => m.Name, o => o.MapFrom(f => f.Body.Name + " DownloadCustomerService"));
            }
        }
    }
}
