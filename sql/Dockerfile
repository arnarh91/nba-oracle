FROM mcr.microsoft.com/dotnet/runtime:8.0

ENV PATH="/opt/grate:${PATH}"

WORKDIR /opt/grate
COPY ../grate .

WORKDIR /sql
COPY . .

RUN chmod +x /opt/grate/grate

ENTRYPOINT ["/opt/grate/grate"]
CMD [""]