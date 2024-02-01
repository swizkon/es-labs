{:ok, connection} = AMQP.Connection.open
{:ok, channel} = AMQP.Channel.open(connection)

AMQP.Queue.declare(channel, "task_queue", durable: true)

message =
  case System.argv do
    []    -> "Hello World!"
    words -> Enum.join(words, " ")
  end


AMQP.Basic.publish(channel, "", "task_queue", "1. " <> message, persistent: true)
AMQP.Basic.publish(channel, "", "task_queue", "2. " <> message, persistent: true)
AMQP.Basic.publish(channel, "", "task_queue", "3. " <> message, persistent: true)
AMQP.Basic.publish(channel, "", "task_queue", "4. " <> message, persistent: true)
AMQP.Basic.publish(channel, "", "task_queue", "5. " <> message, persistent: true)
IO.puts " [x] Sent '#{message}'"

AMQP.Connection.close(connection)
