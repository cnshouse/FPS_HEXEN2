<?php
include("bl_Common.php");
Utils::check_session($_POST['sid']);

$link = Connection::dbConnect();

$sid    = Utils::sanitaze_var($_POST['sid'], $link);
$name   = Utils::sanitaze_var($_POST['name'], $link, $sid);
$id     = Utils::sanitaze_var($_POST['id'], $link, $sid);
$type   = Utils::sanitaze_var($_POST['type'], $link, $sid);
$hash   = Utils::sanitaze_var($_POST['hash'], $link, $sid);
$coins  = Utils::sanitaze_var($_POST['coins'], $link, $sid);
$line   =  Utils::sanitaze_var($_POST['line'], $link, $sid);

$real_hash = Utils::get_secret_hash($name);
if ($real_hash == $hash) {

    if ($type == 0) { //save purchases
        $query = "UPDATE " . PLAYERS_DB . " SET purchases='$line', coins=coins-'$coins' WHERE id='$id'";
        $result = mysqli_query($link, $query) or die(mysqli_error($link));
        if ($result) {
            echo "done";
        }
    }
} else {
    http_response_code(401);
}

mysqli_close($link);