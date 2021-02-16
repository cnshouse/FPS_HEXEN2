<?php
include("bl_Common.php");

$top = safe($_POST['top']);

$link = Connection::dbConnect();

$query = "SELECT * FROM " . PLAYERS_DB . " ORDER by `score` DESC LIMIT " . $top;
$result = mysqli_query($link, $query) or die('Query failed: ' . mysqli_connect_error());

$num_results = mysqli_num_rows($result);

if ($num_results > 0) {
    for ($i = 0; $i < $num_results; $i++) {
        $row = mysqli_fetch_array($result);      
        echo $row['name'] . "|" . $row['nick'] . "|" . $row['kills'] . "|" . $row['deaths'] . "|" . $row['score'] . "|" . $row['status'] . "|\n";        
    }
} else {
    echo "010"; 
}
mysqli_close($link);
?>