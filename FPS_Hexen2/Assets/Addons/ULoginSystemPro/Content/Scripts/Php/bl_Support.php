<?php
include("bl_Common.php");

$name    = safe($_POST['name']);
$title   = safe($_POST['title']);
$content = safe($_POST['content']);
$hash    = safe($_POST['hash']);
$typ     = safe($_POST['type']);
$reply   = safe($_POST['reply']);
$id      = safe($_POST['id']);

$link = dbConnect();

$name    = stripslashes($name);
$name    = mysqli_real_escape_string($link, $name);
$title   = stripslashes($title);
$title   = mysqli_real_escape_string($link, $title);
$content = stripslashes($content);
$content = mysqli_real_escape_string($link, $content);
$reply   = stripslashes($reply);
$reply   = mysqli_real_escape_string($link, $reply);


$real_hash = md5($name . $secretKey);
if ($real_hash == $hash) {
    if ($typ == "1") {
        $sql = "INSERT INTO MyGameTickets (name, title, content) VALUES ('$name', '$title', '$content')";
        if ($check = mysqli_query($link, $sql)) {
            echo "success";
        } else {
            die(mysqli_error($link));
        }
    } else if ($typ == "2") {
        
        $check = mysqli_query($link, "SELECT * FROM MyGameTickets WHERE name ='$name' AND close !='2' ") or die(mysqli_connect_error());
        $numrows = mysqli_num_rows($check);
        if ($numrows == 0) {
            echo "none";
        } else {
            while ($row = mysqli_fetch_assoc($check)) {
                echo "reply";
                echo "|";
                echo $row['content'];
                echo "|";
                echo $row['reply'];
                echo "|";
                echo $row['id'];
                echo "|";
            }
        }
        
    } else if ($typ == "3") {
        
        $query = "SELECT * FROM `MyGameTickets` WHERE close ='0'";
        $result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());
        $num_results = mysqli_num_rows($result);
        
        if ($num_results > 0) {
            for ($i = 0; $i < $num_results; $i++) {
                $row = mysqli_fetch_array($result);
                
                echo $row['title'] . "|" . $row['content'] . "|" . $row['reply'] . "|" . $row['id'] . "|" . $row['name'] . "|\n";
                
            }
        }
    } else if ($typ == "4") {
        
        $check = mysqli_query($link, "UPDATE MyGameTickets SET reply='" . $reply . "', close='1' WHERE id='$id'") or die(mysqli_connect_error());
        if ($check) {
            echo "success";
        }
    } else if ($typ == "5") {
        
        $check = mysqli_query($link, "DELETE FROM MyGameTickets  WHERE id='$id'") or die(mysqli_connect_error());
        if ($check) {
            echo "success";
        }
    } else {
        die("Any type are assigned with this id:" . $typ . " for user: " . $name);
    }
    
} else {
    die("You don't have permission for this! " . $name . " " . $secretKey);
}

mysqli_close($link);
?>