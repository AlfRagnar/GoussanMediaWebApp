function setupVideo() {
  var myPlayer = amp(
    "somethingElse",
    {
      /* Options */
      techOrder: [
        "azureHtml5JS",
        "flashSS",
        "html5FairPlayHLS",
        "silverlightSS",
        "html5",
      ],
      nativeControlsForTouch: false,
      autoplay: false,
      controls: true,
      width: "640",
      height: "400",
      poster: "",
    },
    function () {
      console.log("Good to go!");
      // add an event listener
      this.addEventListener("ended", function () {
        console.log("Finished!");
      });
    }
  );
  myPlayer.src([
    {
      src: "http://samplescdn.origin.mediaservices.windows.net/e0e820ec-f6a2-4ea2-afe3-1eed4e06ab2c/AzureMediaServices_Overview.ism/manifest",
      type: "application/vnd.ms-sstr+xml",
    },
  ]);
}
