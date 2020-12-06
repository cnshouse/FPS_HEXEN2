<?php
include("bl_Common.php");

$currentpass = safe($_POST['password']);
$newpass     = safe($_POST['newpassword']);
$userId      = safe($_POST['id']);
$hash        = safe($_POST['hash']);

$link = dbConnect();

$currentpass = stripslashes($currentpass);
$currentpass = mysqli_real_escape_string($link, $currentpass);
$newpass     = stripslashes($newpass);
$newpass     = mysqli_real_escape_string($link, $newpass);

$real_hash = md5($userId . $secretKey);
if ($real_hash == $hash) {
    
    $check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `id` ='$userId' ") or die(mysqli_connect_error());
    $numrows = mysqli_num_rows($check);
       
    if ($numrows == 0) {
        die("008"); //user not found
    } else {
        $currentpass = md5($currentpass);
        while ($row = mysqli_fetch_assoc($check)) {
            if ($currentpass == $row['password']) {
                $newpass = md5($newpass);
                $update = mysqli_query($link, "UPDATE MyGameDB SET password='" . $newpass . "' WHERE id='$userId'") or die(mysqli_connect_error());
                echo "success";
            } else {
                die("002"); //wrong password
            }
        }
    }
} else {
    die("You don't have permission for this!");
}

mysqli_close($link);
?>