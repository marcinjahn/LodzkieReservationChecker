const player = require('play-sound')(opts = { });

exports.play = function (callback, path) {
    player.play(path, function (err) {
        if (err) callback(null, err.toString());
        callback(null, "Done");
    });
}