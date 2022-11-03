using RabbitMQ.Client;

namespace backend.queue;

// public interface IFhirDataQueue: IModel{};


public static class DIHelper {
    public static void DeclareFhirQueues(this IConnection queue){
        using(var model = queue.CreateModel()) {
            // We are actually supposed to declare all resources in their own queues. For now adding device only.
            model.ExchangeDeclare(exchange: "device",
                                type: ExchangeType.Fanout,
                                durable: true,
                                autoDelete: false,
                                arguments: null);
        }
    }
}

