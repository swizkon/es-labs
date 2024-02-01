
IO.puts "send.exs"

# alias AMQP.Connection, as: Connection

# import AMQP

{:ok, connection} = AMQP.Connection.open
{:ok, channel} = AMQP.Channel.open(connection)

AMQP.Queue.declare(channel, "hello")

message = "Hello World at #{DateTime.utc_now}"
AMQP.Basic.publish(channel, "", "hello", message)
IO.puts message

AMQP.Connection.close(connection)
