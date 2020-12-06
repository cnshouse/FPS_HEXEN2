<?php
include("bl_Common.php");
$link = dbConnect();

$name   = safe($_POST['name']);
$reason = safe($_POST['reason']);
$m_ip   = safe($_POST['myIP']);
$m_by   = safe($_POST['mBy']);
$hash   = safe($_POST['hash']);
$type   = safe($_POST['type']);
$status = safe($_POST['status']);

$real_hash = md5($name . $secretKey);
if ($real_hash == $hash) {
    if ($type == 1) { //Ban
        $check   = mysqli_query($link, "SELECT * FROM BanList WHERE `name`= '$name'");
        $numrows = mysqli_num_rows($check);
        
        if ($numrows == 0) {
            
            $ins = mysqli_query($link, "INSERT INTO  `BanList` (`name` ,  `reason` ,  `myIP` ,  `mBy` ) VALUES ('" . mysqli_real_escape_string($link, $name) . "' ,  '" . mysqli_real_escape_string($link, $reason) . "' ,  '" . mysqli_real_escape_string($link, $m_ip) . "',  '" . mysqli_real_escape_string($link, $m_by) . "') ");
            
            $newstatus = 3; //banned status
            $inst      = mysqli_query($link, "UPDATE MyGameDB SET status='" . $newstatus . "' WHERE name= '$name'");
            
            if ($ins) {
                
                die("success");
            } else {
                die("Error: " . mysqli_connect_error());
            }
        } else {
            echo "This Player is already banned";
        }
    } else if ($type == 2) { //UnBan
        $sql       = "DELETE FROM BanList WHERE name= '$name'";
        $newstatus = 0; //banned status
        $inst      = mysqli_query($link, "UPDATE MyGameDB SET status='" . $newstatus . "' WHERE name= '$name'");
        
        if (mysqli_query($link, $sql)) {
            echo "success";
        } else {
            echo "Error deleting record: " . mysqli_error($conn);
        }
    } else if ($type == 3) {
        $query   = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name`= '$name'") or die(mysqli_error($link));
        $numrows = mysqli_num_rows($query);
        if ($numrows > 0) {
            if (mysqli_query($link, "UPDATE MyGameDB SET status='" . $status . "' WHERE name='$name'") or die(mysqli_error($link))) {
                echo "update";
            }else{
                echo "error";
            }
        } else {
            die("008"); // player with this name not exist
        }
    }
} else {
    echo "you do not have permission to access.";
}

mysqli_close($link);
?>