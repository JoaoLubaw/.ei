(function () {
    const interval = setInterval(() => {
        const wrapper = document.querySelector(".topbar-wrapper");
        if (!wrapper) return;

        const svg = wrapper.querySelector("svg");
        if (svg) {
            const img = document.createElement("img");
            img.src = "/img/logo.svg";
            img.style.height = "36px";
            img.style.width = "auto";
            svg.replaceWith(img);
            clearInterval(interval);
        }
    }, 100);

    (function () {
        window.addEventListener("load", function () {
            setTimeout(function () {
                const links = document.getElementsByTagName("link");

                for (const link of links) {
                    if (link?.rel === "icon") {
                        link.href = "img/favicon.ico";
                    }
                }

                const wrapper =
                    document.getElementsByClassName("topbar-wrapper");
                const t = document.getElementsByClassName(
                    "download-url-wrapper",
                );

                const div = document.createElement("div");
                div.innerHTML = "";

                if (wrapper.length > 0 && t.length > 0) {
                    wrapper[0].insertBefore(div, t[0]);
                }
            }, 10);
        });
    })();

    setTimeout(() => clearInterval(interval), 10000);
})();
