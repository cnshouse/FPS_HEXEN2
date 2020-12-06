<?php
include("bl_Common.php");

$name = $_POST['name'];
$pass = $_POST['password'];

$link = dbConnect();

$name = stripslashes($name);
$pass = stripslashes($pass);
$name = mysqli_real_escape_string($link, $name);
$pass = mysqli_real_escape_string($link, $pass);

if (empty($name)) {
    die("You don't have permission for this");
}

$check = mysqli_query($link, "SELECT * FROM MyGameDB WHERE `name` ='$name' ") or die(mysqli_error($link));
$numrows = mysqli_num_rows($check);

if ($numrows == 0) {
    die("001"); //user not exist
} else {
    $pass = md5($pass);
    while ($row = mysqli_fetch_assoc($check)) {
        if ($pass == $row['password']) {
            if ($row['active'] == "1") {
                echo "success|";
                echo $row['name'] . "|";
                echo $row['nick'] . "|";
                echo $row['kills'] . "|";
                echo $row['deaths'] . "|";
                echo $row['score'] . "|";
                echo $row['playtime'] . "|";
                echo $row['status'] . "|";
                echo $row['id'] . "|";
                echo $row['flist'] . "|";
                echo $row['coins'] . "|";
                echo $row['uIP'] . "|";
                echo $row['clan'] . "|";
                echo $row['clan_invitations'] . "|";
                echo $row['purchases'] . "|" . 
                $row['meta'] . "|";//15

            } else {
                die("007");
            }
            
        } else {
            die("002"); //wrong password
        }
    }
}
mysqli_close($link);
?>