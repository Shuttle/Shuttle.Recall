export type EnvelopeHeader = {
  key: string;
  value: string;
};

export type Event = {
  primitiveEvent: PrimitiveEvent;
  eventEnvelope: EventEnvelope;
  domainEvent: string;
};

export type EventEnvelope = {
  assemblyQualifiedName: string;
  compressionAlgorithm: string;
  encryptionAlgorithm: string;
  event: Uint8Array;
  eventDate: Date;
  eventId: string;
  eventType: string;
  headers: EnvelopeHeader[];
  version: number;
};

export type EventStoreResponse<T> = {
  authorized: boolean;
  items: T[];
};

export type EventType = {
  id: string;
  typeName: string;
};

export type PrimitiveEvent = {
  dateRegistered: Date;
  eventEnvelope: Uint8Array;
  eventId: string;
  eventType: string;
  id: string;
  correlationId?: string | null;
  sequenceNumber: number;
  version: number;
};
