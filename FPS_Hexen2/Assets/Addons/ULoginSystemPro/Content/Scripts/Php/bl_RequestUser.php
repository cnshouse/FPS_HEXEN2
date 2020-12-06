<?php
include("bl_Common.php");

$name = strip_tags($_POST['name']);
$type = strip_tags($_POST['type']);

$link=dbConnect();

$name    = stripslashes($name);
$name    = mysqli_real_escape_string($link, $name);
 
 if($type == "0"){
$check = mysqli_query($link,"SELECT * FROM BanList WHERE name ='$name' ") or die(mysqli_connect_error());
$numrows = mysqli_num_rows($check);
if ($numrows == 0)
{
	die ("This user does not exist in BanList! \n");
}
else
{
	while($row = mysqli_fetch_assoc($check))
	{
                        echo "Exist";
                        echo "|";
                        echo $row['name'];
                        echo "|";
                        echo $row['reason'];
                        echo "|";
                        echo $row['myIP'];
                        echo "|";
                        echo $row['mBy'];
                        echo "|";
                }
}
}else if($type == "1"){
$check = mysqli_query($link,"SELECT * FROM MyGameDB WHERE name ='$name' ") or die(mysqli_connect_error());
$numrows = mysqli_num_rows($check);
if ($numrows == 0)
{
	die ("This user does not exist in DataBase! \n");
}
else
{
	while($row = mysqli_fetch_assoc($check))
	{
                        echo "Exist";
                        echo "|";
                        echo $row['name'];
                        echo "|";
                        echo $row['status'];
                        echo "|";
                        echo $row['uIP'];
                        echo "|";
                        echo $row['score'];
                        echo "|";
                }
}
}
else if($type == "2"){
$check = mysqli_query($link,"SELECT * FROM MyGameDB WHERE name='$name'") or die(mysqli_connect_error());
$numrows = mysqli_num_rows($check);
if ($numrows == 0)
{
	$check = mysqli_query($link,"SELECT * FROM MyGameDB WHERE nick='$name'") or die(mysqli_connect_error());
    $numrows = mysqli_num_rows($check);
	if ($numrows == 0)
{
	die ("User with this name or nick name does not exist in DataBase! \n");
}else{
	while($row = mysqli_fetch_assoc($check))
	{
                        echo "success";
                        echo "|";
                        echo $row['name'];
                        echo "|";
                        echo $row['kills'];
                        echo "|";
                        echo $row['deaths'];
                        echo "|";
                        echo $row['score'];
                        echo "|";
						echo $row['uIP'];
                        echo "|";
						echo $row['status'];
                        echo "|";
						echo $row['playtime'];
                        echo "|";
                        echo $row['nick'];
                        echo "|";
                }
}
}
else
{
	while($row = mysqli_fetch_assoc($check))
	{
                        echo "success";
                        echo "|";
                        echo $row['name'];
                        echo "|";
                        echo $row['kills'];
                        echo "|";
                        echo $row['deaths'];
                        echo "|";
                        echo $row['score'];
                        echo "|";
						echo $row['uIP'];
                        echo "|";
						echo $row['status'];
                        echo "|";
						echo $row['playtime'];
                        echo "|";
                        echo $row['nick'];
                        echo "|";
                }
}
}else if($type == "3"){
	
$result=mysqli_query($link,"SELECT count(*) as total from MyGameDB");
    $data=mysqli_fetch_assoc($result);
    $tablecount =  $data['total'];
	
	$result2 =mysqli_query($link,"SELECT count(*) as total from BanList");
    $data2=mysqli_fetch_assoc($result2);
    $tablecount2 =  $data2['total'];
	
	$result3=mysqli_query($link,"SELECT SUM(playtime) as total from MyGameDB");
    $data3=mysqli_fetch_assoc($result3);
    $tablecount3 =  $data3['total'];
	

$lastp = mysqli_query($link, "SELECT nick FROM MyGameDB ORDER BY `id` DESC LIMIT 1") or die(mysqli_connect_error());
$lastone = mysqli_fetch_assoc($lastp);

echo "info|" . $tablecount . "|" . $lastone["nick"] . "|" . $tablecount2 . "|" . $tablecount3;
}else{
	die('request id not defined');
}
mysqli_close($link);
?>