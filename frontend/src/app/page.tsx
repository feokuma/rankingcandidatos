"use client";

import { FormEvent, useEffect, useState } from "react";

type Candidato = {
  id: string;
  nome: string;
  cidade: string;
  partido: string;
  candidatura: string;
  pontosPositivos: number;
  pontosNegativos: number;
  pontuacao: number;
};

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export default function Home() {
  const [candidatos, setCandidatos] = useState<Candidato[]>([]);
  const [nome, setNome] = useState("");
  const [cidade, setCidade] = useState("");
  const [partido, setPartido] = useState("");
  const [candidatura, setCandidatura] = useState("Prefeito");
  const [erro, setErro] = useState<string | null>(null);
  const [carregando, setCarregando] = useState(true);

  async function carregarRanking() {
    setErro(null);

    try {
      const response = await fetch(`${API_URL}/api/candidatos/ranking`, {
        cache: "no-store",
      });

      if (!response.ok) {
        throw new Error("Não foi possível carregar o ranking.");
      }

      const data = (await response.json()) as Candidato[];
      setCandidatos(data);
    } catch (error) {
      setErro(error instanceof Error ? error.message : "Erro inesperado.");
    } finally {
      setCarregando(false);
    }
  }

  useEffect(() => {
    carregarRanking();
  }, []);

  async function cadastrarCandidato(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setErro(null);

    try {
      const response = await fetch(`${API_URL}/api/candidatos`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          nome,
          cidade,
          partido,
          candidatura,
        }),
      });

      if (!response.ok) {
        const data = (await response.json()) as { erro?: string };
        throw new Error(data.erro ?? "Não foi possível cadastrar o candidato.");
      }

      setNome("");
      setCidade("");
      setPartido("");
      setCandidatura("Prefeito");
      await carregarRanking();
    } catch (error) {
      setErro(error instanceof Error ? error.message : "Erro inesperado.");
    }
  }

  async function avaliar(id: string, tipo: "positiva" | "negativa") {
    setErro(null);

    try {
      const response = await fetch(`${API_URL}/api/candidatos/${id}/avaliacoes`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ tipo }),
      });

      if (!response.ok) {
        const data = (await response.json()) as { erro?: string };
        throw new Error(data.erro ?? "Não foi possível registrar avaliação.");
      }

      await carregarRanking();
    } catch (error) {
      setErro(error instanceof Error ? error.message : "Erro inesperado.");
    }
  }

  return (
    <div className="mx-auto flex min-h-screen w-full max-w-5xl flex-col gap-8 px-4 py-8 sm:px-6 lg:px-8">
      <header className="rounded-2xl bg-slate-900 p-6 text-white shadow-lg">
        <h1 className="text-2xl font-bold">Ranking de Candidatos</h1>
        <p className="mt-2 text-slate-200">
          Cadastre candidatos e marque informações como positivas ou negativas
          para formar um ranking de compatibilidade.
        </p>
      </header>

      <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="text-xl font-semibold text-slate-900">Novo candidato</h2>
        <form className="mt-4 grid gap-4 sm:grid-cols-2" onSubmit={cadastrarCandidato}>
          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
            Nome
            <input
              className="rounded-lg border border-slate-300 px-3 py-2"
              value={nome}
              onChange={(event) => setNome(event.target.value)}
              required
            />
          </label>

          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
            Cidade
            <input
              className="rounded-lg border border-slate-300 px-3 py-2"
              value={cidade}
              onChange={(event) => setCidade(event.target.value)}
              required
            />
          </label>

          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
            Partido
            <input
              className="rounded-lg border border-slate-300 px-3 py-2"
              value={partido}
              onChange={(event) => setPartido(event.target.value)}
            />
          </label>

          <label className="flex flex-col gap-2 text-sm font-medium text-slate-700">
            Candidatura
            <select
              className="rounded-lg border border-slate-300 px-3 py-2"
              value={candidatura}
              onChange={(event) => setCandidatura(event.target.value)}
            >
              <option>Prefeito</option>
              <option>Vereador</option>
              <option>Outro</option>
            </select>
          </label>

          <button
            className="sm:col-span-2 rounded-lg bg-slate-900 px-4 py-3 font-semibold text-white hover:bg-slate-800"
            type="submit"
          >
            Cadastrar candidato
          </button>
        </form>
      </section>

      <section className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-semibold text-slate-900">Ranking atual</h2>
          <button
            className="rounded-lg border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
            onClick={carregarRanking}
            type="button"
          >
            Atualizar
          </button>
        </div>

        {erro && (
          <p className="mb-4 rounded-lg bg-rose-50 px-3 py-2 text-sm text-rose-700">
            {erro}
          </p>
        )}

        {carregando ? (
          <p className="text-slate-600">Carregando ranking...</p>
        ) : candidatos.length === 0 ? (
          <p className="text-slate-600">Nenhum candidato cadastrado ainda.</p>
        ) : (
          <ol className="space-y-3">
            {candidatos.map((candidato, index) => (
              <li
                key={candidato.id}
                className="rounded-xl border border-slate-200 p-4"
              >
                <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <div>
                    <p className="text-sm font-medium text-slate-500">#{index + 1}</p>
                    <h3 className="text-lg font-semibold text-slate-900">{candidato.nome}</h3>
                    <p className="text-sm text-slate-600">
                      {candidato.candidatura} • {candidato.cidade}
                      {candidato.partido ? ` • ${candidato.partido}` : ""}
                    </p>
                    <p className="mt-1 text-sm text-slate-700">
                      Pontuação: <strong>{candidato.pontuacao}</strong> ({candidato.pontosPositivos} positivas / {candidato.pontosNegativos} negativas)
                    </p>
                  </div>

                  <div className="flex gap-2">
                    <button
                      className="rounded-lg bg-emerald-600 px-3 py-2 text-sm font-semibold text-white hover:bg-emerald-500"
                      type="button"
                      onClick={() => avaliar(candidato.id, "positiva")}
                    >
                      + Positiva
                    </button>
                    <button
                      className="rounded-lg bg-rose-600 px-3 py-2 text-sm font-semibold text-white hover:bg-rose-500"
                      type="button"
                      onClick={() => avaliar(candidato.id, "negativa")}
                    >
                      - Negativa
                    </button>
                  </div>
                </div>
              </li>
            ))}
          </ol>
        )}
      </section>
    </div>
  );
}
