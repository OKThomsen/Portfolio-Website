// Drives the animated background: sets body[data-section] as sections cross the
// vertical middle of the viewport, so the CSS wave can drift/fade/tint per section.
(function () {
  var body = document.body;
  body.dataset.section = body.dataset.section || "hero";

  // Enable transitions only after first paint to avoid a load-time flash.
  requestAnimationFrame(function () {
    body.classList.add("bg-ready");
  });

  // Publish the footer's real rendered height so the last full-bleed
  // section (see site.css) can size itself to exactly the space visible
  // above the footer, instead of the footer cutting into a 100svh box and
  // pushing its centered content off-center.
  var footer = document.querySelector(".site-footer");
  if (footer) {
    var updateFooterHeight = function () {
      document.documentElement.style.setProperty(
        "--footer-height",
        footer.offsetHeight + "px"
      );
    };
    updateFooterHeight();
    window.addEventListener("resize", updateFooterHeight);
  }

  var sections = document.querySelectorAll("main section[id]");
  if (!sections.length || !("IntersectionObserver" in window)) {
    return;
  }

  var observer = new IntersectionObserver(
    function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting && entry.target.id) {
          body.dataset.section = entry.target.id;
        }
      });
    },
    // Active band is the middle 10% of the viewport: one section at a time.
    { rootMargin: "-45% 0px -45% 0px", threshold: 0 }
  );

  sections.forEach(function (section) {
    observer.observe(section);
  });
})();
