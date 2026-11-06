// Comportamentos do site. Fica num arquivo próprio porque a Content-Security-Policy
// não permite JavaScript escrito dentro do HTML.
(function () {
    "use strict";
    document.documentElement.classList.add("js");

    // ---------- Menu no celular ----------
    const botaoMenu = document.querySelector(".menu-botao");
    const menu = document.getElementById("menu-principal");
    if (botaoMenu && menu) {
        botaoMenu.addEventListener("click", function () {
            const aberto = menu.classList.toggle("aberto");
            botaoMenu.setAttribute("aria-expanded", String(aberto));
        });
    }

    // ---------- Abas acessíveis (setas do teclado funcionam) ----------
    document.querySelectorAll("[data-abas]").forEach(function (grupo) {
        const abas = Array.from(grupo.querySelectorAll('[role="tab"]'));
        const paineis = abas.map(function (aba) { return document.getElementById(aba.getAttribute("aria-controls")); });

        function ativar(indice, focar) {
            abas.forEach(function (aba, i) {
                const ativa = i === indice;
                aba.setAttribute("aria-selected", String(ativa));
                aba.tabIndex = ativa ? 0 : -1;
                if (paineis[i]) paineis[i].hidden = !ativa;
            });
            if (focar) abas[indice].focus();
        }

        abas.forEach(function (aba, i) {
            aba.addEventListener("click", function () { ativar(i, false); });
            aba.addEventListener("keydown", function (e) {
                if (e.key === "ArrowRight") ativar((i + 1) % abas.length, true);
                if (e.key === "ArrowLeft") ativar((i - 1 + abas.length) % abas.length, true);
            });
        });
        ativar(0, false);
    });

    // ---------- Filtros do catálogo aplicam ao marcar ----------
    document.querySelectorAll(".filtros__form input[type=checkbox], .filtros__form input[type=radio]").forEach(function (campo) {
        campo.addEventListener("change", function () { campo.form.submit(); });
    });

    // ---------- Rede de pontos no topo (decorativa) ----------
    const canvas = document.querySelector(".hero__rede");
    const semMovimento = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (!canvas || !canvas.getContext) return;

    const ctx = canvas.getContext("2d");
    let pontos = [];
    let largura = 0;
    let altura = 0;

    function preparar() {
        const escala = window.devicePixelRatio || 1;
        largura = canvas.clientWidth;
        altura = canvas.clientHeight;
        canvas.width = largura * escala;
        canvas.height = altura * escala;
        ctx.setTransform(escala, 0, 0, escala, 0, 0);
        const quantidade = Math.min(70, Math.round((largura * altura) / 16000));
        pontos = Array.from({ length: quantidade }, function () {
            return {
                x: Math.random() * largura,
                y: Math.random() * altura,
                vx: (Math.random() - 0.5) * 0.25,
                vy: (Math.random() - 0.5) * 0.25
            };
        });
    }

    function desenhar() {
        ctx.clearRect(0, 0, largura, altura);
        for (let i = 0; i < pontos.length; i++) {
            const a = pontos[i];
            for (let j = i + 1; j < pontos.length; j++) {
                const b = pontos[j];
                const d = Math.hypot(a.x - b.x, a.y - b.y);
                if (d < 140) {
                    ctx.strokeStyle = "rgba(160, 196, 255," + (1 - d / 140) * 0.5 + ")";
                    ctx.lineWidth = 1;
                    ctx.beginPath();
                    ctx.moveTo(a.x, a.y);
                    ctx.lineTo(b.x, b.y);
                    ctx.stroke();
                }
            }
            ctx.fillStyle = "rgba(210, 228, 255, 0.85)";
            ctx.beginPath();
            ctx.arc(a.x, a.y, 2, 0, Math.PI * 2);
            ctx.fill();
        }
    }

    function animar() {
        pontos.forEach(function (p) {
            p.x += p.vx;
            p.y += p.vy;
            if (p.x < 0 || p.x > largura) p.vx *= -1;
            if (p.y < 0 || p.y > altura) p.vy *= -1;
        });
        desenhar();
        requestAnimationFrame(animar);
    }

    preparar();
    window.addEventListener("resize", function () { preparar(); desenhar(); });
    if (semMovimento) desenhar(); else animar();
})();
