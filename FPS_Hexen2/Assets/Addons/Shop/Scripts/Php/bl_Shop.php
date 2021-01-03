<?php
include("bl_Common.php");
$link = dbConnect();

$name   = safe($_POST['name']);
$id     = safe($_POST['id']);
$hash   = safe($_POST['hash']);
$type   = safe($_POST['type']);
$line = safe($_POST['line']);
$coins = safe($_POST['coins']);

$name     = stripslashes($name);
$name     = mysqli_real_escape_string($link, $name);
$type     = stripslashes($type);
$type     = mysqli_real_escape_string($link, $type);

$real_hash = md5($name . $secretKey);
if ($real_hash == $hash) {

    if($type == 0){//save purchases
        $query = "UPDATE MyGameDB SET purchases='$line', coins=coins-'$coins' WHERE id='$id'";
        $result = mysqli_query($link,$query) or die(mysqli_error($link));
        if($result){
            echo "done";
        }
    }
} else {
    echo "you do not have permission to access.";
}

mysqli_close($link);
?>