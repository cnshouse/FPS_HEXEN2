<?php
include("bl_Common.php");

$link = Connection::dbConnect();

$code = Utils::sanitaze_var($_GET['code'], $link);

if (!empty($code) && isset($code)) {
    
    $result = mysqli_query($link, "SELECT id FROM " . PLAYERS_DB . " WHERE verify='$code'");
    
    if (mysqli_num_rows($result) > 0) {
        $count = mysqli_query($link, "SELECT id FROM " . PLAYERS_DB . " WHERE verify='$code' and active='0'");
        
        if (mysqli_num_rows($count) == 1) {
            mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET active='1', verify='done' WHERE verify='$code'");
			$msg = "<div style=\"background-color: #101010; position: fixed;top: 0;left: 0;bottom: 0;right: 0;overflow: auto; color: #e2e2e2;\"><center><div style=\"height: 100%;padding-top: 25%;letter-spacing: 2px;\">"
			. "YOUR ACCOUNT IS ACTIVATED, YOU CAN SIGN IN NOW!</div></center></div>";          
        } else {
            $msg = "<div style=\"background-color: #101010; position: fixed;top: 0;left: 0;bottom: 0;right: 0;overflow: auto; color: #e2e2e2;\"><center><div style=\"height: 100%;padding-top: 25%;letter-spacing: 2px;\">"
			. "YOUR ACCOUNT IS ACTIVE, YOU DON'T NEED ACTIVE AGAIN :)</div></center></div>";
        }
        
    } else {
        $msg = "Wrong activation code.";
    }
    die($msg);
}
?>