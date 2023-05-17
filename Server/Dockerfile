FROM registry.heroiclabs.com/heroiclabs/nakama:3.16.0

ADD --chmod=555 pre_start.sh /nakama/
ADD --chmod=555 start.sh /nakama/

ENTRYPOINT /nakama/start.sh
