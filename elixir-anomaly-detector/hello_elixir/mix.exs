defmodule HelloElixir.MixProject do
  use Mix.Project

  def project do
    [
      app: :hello_elixir,
      version: "0.1.0",
      elixir: "~> 1.16",
      start_permanent: Mix.env() == :prod,
      deps: deps()
    ]
  end

  # Run "mix help compile.app" to learn about applications.
  def application do
    [
      extra_applications: [:logger],
      mod: {HelloElixir.Application, []}
    ]
  end

  # Run "mix help deps" to learn about dependencies.
  defp deps do
    [
      {:spear, "~> 1.3"},
      {:jason, "~> 1.0"}
    ]
  end
end
