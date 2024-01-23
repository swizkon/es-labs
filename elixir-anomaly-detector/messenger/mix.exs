defmodule Messenger.MixProject do
  use Mix.Project

  def project do
    [
      app: :messenger,
      version: "0.1.0",
      elixir: "~> 1.16",
      start_permanent: Mix.env() == :prod,
      deps: deps()
    ]
  end

  # Run "mix help compile.app" to learn about applications.
  def application do
    [
      extra_applications: [:logger, :bamboo, :bamboo_smtp],
      mod: {Messenger.Application, []}
    ]
  end

  # Run "mix help deps" to learn about dependencies.
  defp deps do
    [
      {:bamboo, "~> 2.2.0"},
      {:bamboo_smtp, "~> 4.1.0"}
    ]
  end
end
