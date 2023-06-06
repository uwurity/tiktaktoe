# Build TypeScript module

FROM node:alpine as node-builder

WORKDIR /backend

COPY . .

RUN corepack enable
RUN pnpm install
RUN pnpm build

# Prepare Nakama

FROM registry.heroiclabs.com/heroiclabs/nakama:3.16.0 as server
WORKDIR /ts
ENV TSFILE=tailscale_1.42.0_amd64.tgz
RUN curl -sf -o ${TSFILE} https://pkgs.tailscale.com/stable/${TSFILE} && \
  tar xzf ${TSFILE} --strip-components=1

RUN apt update && apt install -y ca-certificates iptables && rm -rf /var/cache/apt

WORKDIR /

ADD --chmod=555 ts_start.sh /ts/start.sh
RUN mkdir -p /var/run/tailscale /var/cache/tailscale /var/lib/tailscale

ADD --chmod=555 pre_start.sh /nakama/
ADD --chmod=555 start.sh /nakama/

COPY --from=node-builder /backend/data/modules/index.js /nakama/

ENTRYPOINT /nakama/start.sh
