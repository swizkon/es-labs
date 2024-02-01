defmodule SmtpSniffer.Supervisor do
  use Supervisor

  def start_link(opts) do
    IO.puts "Starting SmtpSniffer.Supervisor"
    Supervisor.start_link(__MODULE__, :ok, opts)
  end

  def init(:ok) do

    IO.puts "Starting SmtpSniffer.Supervisor.init"
    # children = [
    #   {SmtpSniffer.Server, [id: :gen_smtp_server, start: {SmtpSniffer.Server, :start_link, []}]}
    # ]

    children = [
      {SmtpSniffer.Server, [id: :gen_smtp_server, start: {SmtpSniffer.Server, :start_link, [Application.get_env(:smtp_sniffer, :smtp_opts)]}]}
    ]

    Supervisor.init(children, strategy: :one_for_one)
  end
end
