<?php
include("bl_Common.php");

$name     = safe($_POST['name']);
$typ     = safe($_POST['typ']);

$link = dbConnect();

$name     = stripslashes($name);
$name     = mysqli_real_escape_string($link, $name);
$typ     = stripslashes($typ);
$typ     = mysqli_real_escape_string($link, $typ);

if($typ == "0"){
$query = "SELECT * FROM `BanList` ORDER by `id` DESC";
$result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());

$num_results = mysqli_num_rows($result);
if ($num_results > 0) {
    echo "result\n";
    for ($i = 0; $i < $num_results; $i++) {
        $row = mysqli_fetch_array($result);
        echo $row['name'] . "|" . $row['reason'] . "|" . $row['myIP'] . "|" . $row['mBy'] . "|\n";
    }
} else {
    echo "empty";
}
}else if($typ == "1"){
	 $check2   = mysqli_query($link, "SELECT * FROM BanList WHERE `name`= '$name'");
        $numrows2 = mysqli_num_rows($check2);
        if ($numrows2 != 0) {
            echo "yes";
        }
}
mysqli_close($link);
?>