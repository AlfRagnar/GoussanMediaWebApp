window.streamVideo = function (id, url) {
  var myOptions = {
    autoplay: true,
    controls: true,
    width: "800",
    height: "500",
  };

  var myPlayer = amp(id, myOptions);
  myPlayer.src([{ src: url, type: "application/vnd.ms-sstr+xml" }]);
};
