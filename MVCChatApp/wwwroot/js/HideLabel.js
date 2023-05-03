function hide() {
    var timer;
    for (var i = 0; i < 10; i++) {
        if (timer == 5) {
            document.getElementById("redirectionLabel").style.display = "none";
        }
        timer++;
    }
}