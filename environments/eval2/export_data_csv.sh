#!/bin/sh
echo $1
echo $2
STYX=~/go/bin/styx
TIME_SPAN=$1
OUTPUT_DIR=$2
EX="ex2"
TIME=$(date "+%Y%m%d-%H%M%S")
if [ ! -d $OUTPUT_DIR/$TIME-$EX ]; then
  mkdir -p $OUTPUT_DIR/$TIME-$EX
fi
OUTPUT_DIR=$OUTPUT_DIR/$TIME-$EX
echo $OUTPUT_DIR

#$STYX -d $TIME_SPAN 'bms_business_violation_total' > $OUTPUT_DIR/bms_business_violation_total.csv
echo "Start scrach csv from promethus"
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_1"}'  > $OUTPUT_DIR/scale_business_web_1.csv
echo "Step1"
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_2"}'  > $OUTPUT_DIR/scale_business_web_1.csv
echo "Step2"
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_3"}'  > $OUTPUT_DIR/scale_business_web_1.csv
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'avg(rate(container_cpu_usage_seconds_total[1m])) by (container_label_com_docker_swarm_service_name) * 100'  > $OUTPUT_DIR/cpu_usage.csv
echo "Step3"
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'avg(container_memory_rss) by (container_label_com_docker_swarm_service_name)'  > $OUTPUT_DIR/memory_usage.csv
echo "Step4"
$STYX -d $TIME_SPAN --prometheus http://manager-01:9090 'avg(rate(container_fs_writes_bytes_total[1m])) by (container_label_com_docker_swarm_service_name)'  > $OUTPUT_DIR/io_write_usage.csv
echo "Step5"
echo "Stop scrach csv from promethus"

#docker run --rm -ti --net=host taskrabbit/elasticsearch-dump --input=http://localhost:9200/fluentd-*  --output=$ --type=data | gzip > $OUTPUT_DIR/logs.json.gz
