defmodule SmtpSniffer do

  use Application

#  @impl true
  def start(_type, _args) do

    IO.puts "Starting SmtpSniffer"

    children = [
      # Starts a worker by calling: HelloElixir.Worker.start_link(arg)
      # {HelloElixir.Worker, arg}
    ]

    # See https://hexdocs.pm/elixir/Supervisor.html
    # for other strategies and supported options
    opts = [strategy: :one_for_one, name: SmtpSniffer.Supervisor]
    link = Supervisor.start_link(children, opts)
    IO.inspect link

    link
  end

  # def start(_type, _args) do
  #   SmtpSniffer.Supervisor.start_link(name: SmtpSniffer.Supervisor)
  # end

end
