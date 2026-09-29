"use strict";

const elements = {
  table: document.querySelector("#table-select"),
  reload: document.querySelector("#reload-tables"),
  tableHint: document.querySelector("#table-hint"),
  allColumns: document.querySelector("#all-columns"),
  columnList: document.querySelector("#column-list"),
  filterList: document.querySelector("#filter-list"),
  orderList: document.querySelector("#order-list"),
  addFilter: document.querySelector("#add-filter"),
  addOrder: document.querySelector("#add-order"),
  generate: document.querySelector("#generate"),
  output: document.querySelector("#sql-output"),
  copy: document.querySelector("#copy-sql"),
  status: document.querySelector("#status")
};

const operators = {
  equal: "Igual a",
  notEqual: "Diferente de",
  contains: "Contém",
  lessThan: "Menor que",
  lessThanOrEqual: "Menor ou igual a",
  greaterThan: "Maior que",
  greaterThanOrEqual: "Maior ou igual a",
  isNull: "Está vazio (NULL)",
  isNotNull: "Não está vazio (NOT NULL)"
};

const state = {
  columns: [],
  filters: [],
  orders: [],
  requestId: 0,
  revision: 0,
  busy: false,
  sql: ""
};

function option(value, label) {
  const item = document.createElement("option");
  item.value = value;
  item.textContent = label;
  return item;
}

function setStatus(message, kind = "") {
  elements.status.textContent = message;
  elements.status.className = `status-message${kind ? ` ${kind}` : ""}`;
}

function clearResult() {
  ++state.revision;
  state.sql = "";
  elements.output.textContent = "-- Configure a consulta e clique em Gerar consulta SQL.";
  elements.copy.disabled = true;
  setStatus("Consulta pronta para gerar.");
}

function setEmpty(container, message) {
  const paragraph = document.createElement("p");
  paragraph.className = "empty-inline";
  paragraph.textContent = message;
  container.replaceChildren(paragraph);
}

function columnType(columnName) {
  return state.columns.find(column => column.name === columnName)?.type ?? "";
}

function typeGroup(type) {
  if (["text", "character varying", "character"].includes(type)) return "text";
  if (["smallint", "integer", "bigint", "numeric", "real", "double precision"].includes(type)) return "number";
  if (type === "boolean") return "boolean";
  if (type === "uuid") return "uuid";
  if (type === "date") return "date";
  if (type === "time without time zone") return "time";
  if (["time with time zone", "timestamp without time zone", "timestamp with time zone"].includes(type)) return "datetime";
  return "other";
}

function operatorsFor(type) {
  const group = typeGroup(type);
  const equality = ["equal", "notEqual"];
  const comparison = ["lessThan", "lessThanOrEqual", "greaterThan", "greaterThanOrEqual"];
  const nullChecks = ["isNull", "isNotNull"];
  if (group === "text") return [...equality, "contains", ...comparison, ...nullChecks];
  if (["number", "date", "time", "datetime"].includes(group)) return [...equality, ...comparison, ...nullChecks];
  if (["boolean", "uuid"].includes(group)) return [...equality, ...nullChecks];
  return nullChecks;
}

function columnSelect() {
  const select = document.createElement("select");
  select.setAttribute("aria-label", "Coluna");
  for (const column of state.columns) select.append(option(column.name, column.name));
  return select;
}

function renderColumns() {
  elements.columnList.replaceChildren();
  for (const column of state.columns) {
    const label = document.createElement("label");
    label.className = "column-option";
    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.value = column.name;
    checkbox.disabled = elements.allColumns.checked;
    checkbox.addEventListener("change", clearResult);
    const name = document.createElement("span");
    name.textContent = column.name;
    const type = document.createElement("small");
    type.textContent = column.type;
    type.title = column.type;
    label.append(checkbox, name, type);
    elements.columnList.append(label);
  }
  if (state.columns.length === 0) setEmpty(elements.columnList, "Esta tabela não tem colunas disponíveis.");
}

function removeRow(items, item, container, emptyMessage) {
  items.splice(items.indexOf(item), 1);
  item.element.remove();
  if (items.length === 0) setEmpty(container, emptyMessage);
  clearResult();
}

function addFilter() {
  if (state.columns.length === 0) return;
  if (state.filters.length === 0) elements.filterList.replaceChildren();

  const row = document.createElement("div");
  row.className = "rule-row";
  const column = columnSelect();
  const operatorSelect = document.createElement("select");
  operatorSelect.setAttribute("aria-label", "Operador");
  const remove = document.createElement("button");
  remove.type = "button";
  remove.className = "remove-button";
  remove.setAttribute("aria-label", "Remover filtro");
  remove.title = "Remover filtro";
  remove.textContent = "×";
  const item = { element: row, column, operator: operatorSelect, value: null };

  function updateValue() {
    const oldValue = item.value;
    const selectedOperator = operatorSelect.value;
    const group = typeGroup(columnType(column.value));
    let control;

    if (["isNull", "isNotNull"].includes(selectedOperator)) {
      control = document.createElement("input");
      control.type = "text";
      control.className = "rule-value empty-value";
      control.disabled = true;
      control.setAttribute("aria-hidden", "true");
      control.tabIndex = -1;
    } else if (group === "boolean") {
      control = document.createElement("select");
      control.append(option("true", "Verdadeiro"), option("false", "Falso"));
      control.className = "rule-value";
      control.setAttribute("aria-label", "Valor booleano");
      control.addEventListener("change", clearResult);
    } else {
      control = document.createElement("input");
      control.className = "rule-value";
      control.type = group === "number" ? "number" : group === "date" ? "date" : group === "time" ? "time" : "text";
      if (group === "number") control.step = "any";
      if (group === "datetime") control.placeholder = "AAAA-MM-DD HH:mm:ss";
      if (group === "uuid") control.placeholder = "UUID";
      control.setAttribute("aria-label", "Valor do filtro");
      control.addEventListener("input", clearResult);
    }

    if (oldValue) oldValue.replaceWith(control);
    else row.insertBefore(control, remove);
    item.value = control;
    clearResult();
  }

  function updateOperators() {
    const previous = operatorSelect.value;
    operatorSelect.replaceChildren();
    for (const key of operatorsFor(columnType(column.value))) {
      operatorSelect.append(option(key, operators[key]));
    }
    if ([...operatorSelect.options].some(item => item.value === previous)) operatorSelect.value = previous;
    updateValue();
  }

  column.addEventListener("change", updateOperators);
  operatorSelect.addEventListener("change", updateValue);
  remove.addEventListener("click", () => removeRow(state.filters, item, elements.filterList, "Nenhum filtro. A consulta incluirá todas as linhas."));
  row.append(column, operatorSelect, remove);
  state.filters.push(item);
  elements.filterList.append(row);
  updateOperators();
}

function addOrder() {
  if (state.columns.length === 0) return;
  if (state.orders.length === 0) elements.orderList.replaceChildren();

  const row = document.createElement("div");
  row.className = "rule-row order-row";
  const column = columnSelect();
  const direction = document.createElement("select");
  direction.setAttribute("aria-label", "Direção da ordenação");
  direction.append(option("ASC", "Crescente"), option("DESC", "Decrescente"));
  const remove = document.createElement("button");
  remove.type = "button";
  remove.className = "remove-button";
  remove.setAttribute("aria-label", "Remover ordenação");
  remove.title = "Remover ordenação";
  remove.textContent = "×";
  const item = { element: row, column, direction };
  column.addEventListener("change", clearResult);
  direction.addEventListener("change", clearResult);
  remove.addEventListener("click", () => removeRow(state.orders, item, elements.orderList, "Sem ordenação definida."));
  row.append(column, direction, remove);
  state.orders.push(item);
  elements.orderList.append(row);
  clearResult();
}

function resetTableDetails() {
  state.columns = [];
  state.filters = [];
  state.orders = [];
  elements.allColumns.checked = true;
  elements.allColumns.disabled = true;
  elements.addFilter.disabled = true;
  elements.addOrder.disabled = true;
  elements.generate.disabled = true;
  setEmpty(elements.columnList, "Carregando colunas...");
  setEmpty(elements.filterList, "Nenhum filtro. A consulta incluirá todas as linhas.");
  setEmpty(elements.orderList, "Sem ordenação definida.");
  clearResult();
}

async function requestJson(url, options) {
  const response = await fetch(url, options);
  const body = await response.text();
  let data;
  try { data = body ? JSON.parse(body) : null; }
  catch { data = body; }
  if (!response.ok) {
    const validationErrors = data && typeof data === "object" && data.errors
      ? Object.values(data.errors).flat().join(" ")
      : "";
    throw new Error(validationErrors || data?.detail || data?.title || `Erro HTTP ${response.status}.`);
  }
  return data;
}

async function loadColumns() {
  const table = elements.table.value;
  const requestId = ++state.requestId;
  resetTableDetails();
  if (!table) {
    setEmpty(elements.columnList, "Selecione uma tabela para ver suas colunas.");
    setStatus("Aguardando uma tabela.");
    return;
  }
  elements.tableHint.textContent = "Carregando colunas...";
  try {
    const data = await requestJson(`/api/Columns?tableName=${encodeURIComponent(table)}`);
    if (requestId !== state.requestId) return;
    if (!Array.isArray(data?.columns)) throw new Error("Resposta de colunas inválida.");
    state.columns = data.columns;
    renderColumns();
    const enabled = state.columns.length > 0;
    elements.allColumns.disabled = !enabled;
    elements.addFilter.disabled = !enabled;
    elements.addOrder.disabled = !enabled;
    elements.generate.disabled = !enabled;
    elements.tableHint.textContent = `${state.columns.length} coluna(s) disponível(is).`;
    setStatus(enabled ? "Configure a consulta e clique em Gerar consulta SQL." : "Esta tabela não tem colunas disponíveis.");
  } catch (error) {
    if (requestId !== state.requestId) return;
    setEmpty(elements.columnList, "Não foi possível carregar as colunas.");
    elements.tableHint.textContent = error.message;
    elements.tableHint.classList.add("error");
    setStatus("Falha ao carregar colunas.", "error");
  }
}

async function loadTables() {
  const previous = elements.table.value;
  const requestId = ++state.requestId;
  resetTableDetails();
  elements.table.disabled = true;
  elements.table.replaceChildren(option("", "Carregando tabelas..."));
  elements.tableHint.classList.remove("error");
  elements.tableHint.textContent = "Conectando à API...";
  try {
    const data = await requestJson("/api/Tables");
    if (requestId !== state.requestId) return;
    if (!Array.isArray(data?.name)) throw new Error("Resposta de tabelas inválida.");
    elements.table.replaceChildren(option("", "Selecione uma tabela"));
    for (const table of data.name) elements.table.append(option(table, table));
    elements.table.disabled = data.name.length === 0;
    if (data.name.length === 0) {
      elements.tableHint.textContent = "Nenhuma tabela no esquema público. Crie uma tabela no PostgreSQL e atualize a lista.";
      setEmpty(elements.columnList, "Nenhuma tabela disponível.");
      setStatus("Crie uma tabela para começar.");
      return;
    }
    elements.table.value = data.name.includes(previous) ? previous : "";
    elements.tableHint.textContent = `${data.name.length} tabela(s) disponível(is).`;
    if (elements.table.value) await loadColumns();
    else {
      setEmpty(elements.columnList, "Selecione uma tabela para ver suas colunas.");
      setStatus("Selecione uma tabela para começar.");
    }
  } catch (error) {
    if (requestId !== state.requestId) return;
    elements.table.replaceChildren(option("", "API indisponível"));
    elements.tableHint.textContent = `Não foi possível carregar as tabelas: ${error.message}`;
    elements.tableHint.classList.add("error");
    setEmpty(elements.columnList, "Não foi possível conectar à API.");
    setStatus("Confira se a API e o PostgreSQL estão em execução.", "error");
  }
}

function filterValue(item) {
  const group = typeGroup(columnType(item.column.value));
  const text = item.value.value.trim();
  if (!text && group !== "boolean") throw new Error(`Informe um valor para o filtro da coluna ${item.column.value}.`);
  if (group === "boolean") return text === "true";
  if (group === "number") {
    const number = Number(text);
    if (!Number.isFinite(number) || (Number.isInteger(number) && !Number.isSafeInteger(number))) {
      throw new Error(`Informe um número válido para ${item.column.value}.`);
    }
    return number;
  }
  return text;
}

function buildPayload() {
  const columns = elements.allColumns.checked
    ? []
    : [...elements.columnList.querySelectorAll("input:checked")].map(input => input.value);
  if (!elements.allColumns.checked && columns.length === 0) {
    throw new Error("Selecione ao menos uma coluna ou marque Todas as colunas.");
  }
  const where = state.filters.map(item => {
    const condition = { column: item.column.value, operator: item.operator.value };
    if (!["isNull", "isNotNull"].includes(condition.operator)) condition.value = filterValue(item);
    return condition;
  });
  const orderBy = state.orders.map(item => ({ column: item.column.value, direction: item.direction.value }));
  return { table: elements.table.value, columns, where, orderBy };
}

async function generateSql() {
  if (state.busy || !elements.table.value) return;
  let payload;
  try { payload = buildPayload(); }
  catch (error) { setStatus(error.message, "error"); return; }

  state.busy = true;
  const revision = state.revision;
  elements.generate.disabled = true;
  setStatus("Gerando consulta...");
  try {
    const data = await requestJson("/api/Queries/Generate", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });
    if (revision !== state.revision) return;
    const sql = typeof data === "string" ? data : data?.sql;
    if (typeof sql !== "string" || !sql) throw new Error("A API não retornou uma consulta SQL.");
    state.sql = sql;
    elements.output.textContent = sql;
    elements.copy.disabled = false;
    setStatus("Consulta gerada. Revise e copie o SQL.", "success");
  } catch (error) {
    if (revision !== state.revision) return;
    state.sql = "";
    elements.output.textContent = "-- Não foi possível gerar a consulta.";
    elements.copy.disabled = true;
    setStatus(error.message, "error");
  } finally {
    state.busy = false;
    elements.generate.disabled = state.columns.length === 0;
  }
}

elements.table.addEventListener("change", () => {
  elements.tableHint.classList.remove("error");
  loadColumns();
});
elements.reload.addEventListener("click", loadTables);
elements.allColumns.addEventListener("change", () => {
  for (const input of elements.columnList.querySelectorAll("input")) input.disabled = elements.allColumns.checked;
  clearResult();
});
elements.addFilter.addEventListener("click", addFilter);
elements.addOrder.addEventListener("click", addOrder);
elements.generate.addEventListener("click", generateSql);
elements.copy.addEventListener("click", async () => {
  try {
    await navigator.clipboard.writeText(state.sql);
    setStatus("SQL copiado para a área de transferência.", "success");
  } catch {
    setStatus("Não foi possível copiar automaticamente. Selecione o texto da consulta para copiar.", "error");
  }
});

loadTables();
